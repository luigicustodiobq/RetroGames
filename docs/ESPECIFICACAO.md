# RetroGames — Especificação do Módulo de Compras (Desafio, Versão Básica)

Complemento da especificação **RetroGames — Arquitetura e Stack** (documento do professor) para as histórias **C01 a C05**. Tudo o que não está descrito aqui segue a especificação original: arquitetura **Controller → Service → Repository**, uma tabela por consulta SQL (sem `JOIN`), composição com LINQ no Service, transações com `TransactionScope` só no Service, envelope `{ responseCode, errorMessage, data }` e sessão por cookie HttpOnly.

Sprint: **23/11/2026 a 29/11/2026** · Responsável: `luigicustodiobq`.

---

## 1. Escopo

| Código | História | Situação |
| --- | --- | --- |
| C01 | Como **visitante**, quero **criar minha conta de cliente**, para **comprar e acompanhar meus pedidos**. | Concluída |
| C02 | Como **cliente logado**, quero **ver Carrinho e Meus pedidos no menu**, para **chegar rápido ao que me interessa**. | Concluída |
| C03 | Como **cliente**, quero **montar um carrinho e ajustar as quantidades**, para **revisar a compra antes de finalizar**. | Concluída |
| C04 | Como **cliente**, quero **finalizar a compra**, para **garantir os jogos com o preço atual e o estoque reservado**. | Concluída |
| C05 | Como **cliente**, quero **consultar meus pedidos**, para **acompanhar a situação e conferir o que paguei**. | Concluída |

Fora do escopo (versões estendidas): pagamento, alteração de situação pelo administrador, cancelamento pelo cliente.

---

## 2. Modelo de dados

### 2.1 Tabelas novas

**`tb_situacao_pedido`** — domínio com chave fixa (sem `AUTO_INCREMENT`).

| Coluna | Tipo | Restrição |
| --- | --- | --- |
| `co_situacao_pedido` | INT | PK |
| `ds_situacao_pedido` | VARCHAR(50) | NOT NULL, UNIQUE |

Valores (iguais a `SituacaoPedidoIds`): 1 Pendente · 2 Pago · 3 Enviado · 4 Cancelado.

**`tb_pedido`**

| Coluna | Tipo | Restrição |
| --- | --- | --- |
| `co_seq_pedido` | INT | PK, AUTO_INCREMENT |
| `dt_pedido` | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP (gravada em UTC pelo Service) |
| `vl_total` | DECIMAL(10,2) | NOT NULL (calculado no Service) |
| `co_usuario` | INT | NOT NULL, FK → `tb_usuario` |
| `co_situacao_pedido` | INT | NOT NULL, FK → `tb_situacao_pedido` |

**`tb_item_pedido`**

| Coluna | Tipo | Restrição |
| --- | --- | --- |
| `co_seq_item_pedido` | INT | PK, AUTO_INCREMENT |
| `qt_item` | INT | NOT NULL |
| `vl_unitario` | DECIMAL(10,2) | NOT NULL (preço do jogo no momento da compra) |
| `co_pedido` | INT | NOT NULL, FK → `tb_pedido` |
| `co_jogo` | INT | NOT NULL, FK → `tb_jogo` |
| — | — | UNIQUE (`co_pedido`, `co_jogo`) |

Sem triggers, functions, `CHECK` ou `ON DELETE CASCADE/SET NULL`.

### 2.2 Relacionamentos

```
tb_usuario         1 ── N  tb_pedido
tb_situacao_pedido 1 ── N  tb_pedido
tb_pedido          1 ── N  tb_item_pedido
tb_jogo            1 ── N  tb_item_pedido
```

### 2.3 Scripts

| Script | Uso |
| --- | --- |
| `database/01_schema.sql` + `02_seed.sql` | Banco novo e vazio (já incluem as tabelas de compras e as 4 situações). Validados num banco vazio temporário. |
| `database/04_modulo_compras.sql` | Banco **já existente**: cria só as tabelas novas (`CREATE TABLE IF NOT EXISTS`) e as situações (`INSERT IGNORE`). Idempotente; não altera usuários (o administrador continua), jogos nem plataformas. |

### 2.4 Classes por camada

| Tabela | Model | Repository | Service | Controller |
| --- | --- | --- | --- | --- |
| `tb_situacao_pedido` | `SituacaoPedido`, `SituacaoPedidoIds` | `ISituacaoPedidoRepository` (`ListarAsync`) | `PedidoService` | — |
| `tb_pedido` | `Pedido` | `IPedidoRepository` (`InserirAsync`, `ListarPorUsuarioAsync`, `BuscarPorIdAsync`) | `IPedidoService` / `PedidoService` | `PedidoController` |
| `tb_item_pedido` | `ItemPedido` | `IItemPedidoRepository` (`InserirAsync`, `ListarPorPedidosAsync`) | `PedidoService` | `PedidoController` |
| `tb_jogo` (alterada) | `Jogo` | `IJogoRepository.AtualizarEstoqueAsync` (novo); `AlterarAsync` agora grava `st_ativo` | `PedidoService`, `JogoService` | — |
| `tb_usuario` | `Usuario` | `IUsuarioRepository` (já existente) | `IAutenticacaoService.CadastrarClienteAsync` | `AutenticacaoController` |

Todos os repositórios novos estão em `IRepositorySet` / `RepositorySet` e registrados em `InjecaoDependenciaConfig`.

---

## 3. Endpoints novos

| Método | Rota | Acesso | Corpo | Sucesso | Erros |
| --- | --- | --- | --- | --- | --- |
| POST | `/api/Autenticacao/cadastrar` | Público | `{ nome, email, senha }` | 201 + `UsuarioLogadoResponseApiModel`; inicia a sessão (cookie `retrogames.sessao`) | 400 (3), 409 (106) |
| POST | `/api/Pedido` | Logado | `{ itens: [{ idJogo, quantidade }] }` | 201 + `PedidoResponseApiModel` + `Location` | 400 (3), 400 (110), 401 (4), 409 (111) |
| GET | `/api/Pedido/meus` | Logado | — | 200 + lista de `PedidoResponseApiModel` | 401 (4) |
| GET | `/api/Pedido/{id}` | Logado | — | 200 + `PedidoResponseApiModel` | 401 (4), 403 (113), 404 (109) |

Alteração em endpoint existente: `PUT /api/Jogo/{id}` passa a gravar o campo `ativo` (permite **reativar** um jogo desativado logicamente).

`PedidoResponseApiModel`: `id`, `dataPedido` (UTC, ISO 8601), `valorTotal`, `idSituacaoPedido`, `descricaoSituacao`, `itens[]` com `idJogo`, `nomeJogo`, `quantidade`, `valorUnitario`, `subtotal`.

### 3.1 Validação de formato (HTTP 400, `responseCode` 3)

| Validator | Regras |
| --- | --- |
| `CadastrarClienteRequestValidator` | Nome obrigatório, até 150 · E-mail obrigatório, válido, até 150 · Senha obrigatória, mínimo 6 |
| `FinalizarPedidoRequestValidator` | Lista não vazia · cada `idJogo > 0` · cada `quantidade` de 1 a 10 · **quantidade total de cada jogo repetido (soma) de 1 a 10** |

---

## 4. Códigos de erro (106 a 113)

| Código | HTTP | Constante | Mensagem |
| --- | --- | --- | --- |
| 106 | 409 | `EMAIL_JA_CADASTRADO` | Já existe uma conta com este e-mail. |
| 109 | 404 | `PEDIDO_NAO_ENCONTRADO` | Pedido não encontrado. |
| 110 | 400 | `JOGO_INDISPONIVEL` | Um ou mais jogos não estão disponíveis para venda. |
| 111 | 409 | `ESTOQUE_INSUFICIENTE` | Estoque insuficiente. |
| 113 | 403 | `PEDIDO_DE_OUTRO_USUARIO` | Este pedido pertence a outro usuário. |

Os códigos 107, 108 e 112 pertencem às versões estendidas e não são usados na Versão Básica.

---

## 5. Regras de negócio

| Regra | Descrição | Onde |
| --- | --- | --- |
| RC01 | Cadastro público cria apenas **Cliente** (`co_perfil = 2`), ativo, com senha em hash (`PasswordHasher`). E-mail único: 106, inclusive na corrida do UNIQUE (`RegistroDuplicadoException`). | `AutenticacaoService.CadastrarClienteAsync` |
| RC02 | Após o cadastro, a sessão é iniciada automaticamente. | `AutenticacaoController.Cadastrar` |
| RC03 | O carrinho fica no navegador (`localStorage`, chave `retrogames_carrinho`) e guarda só itens; nenhum dado de sessão (RS01). Quantidade por jogo entre 1 e `min(10, estoque)`. | `CarrinhoProvider` |
| RC04 | O usuário do pedido vem **sempre da sessão** (`IUsuarioAtualService`), nunca do corpo. | `PedidoService` |
| RC05 | O preço vem **sempre do banco**; `vl_unitario` e `vl_total` são calculados no Service. | `PedidoService.FinalizarCompraAsync` |
| RC06 | Jogo inexistente ou inativo: 110. Quantidade maior que o estoque: 111. Nada é gravado. | `PedidoService.FinalizarCompraAsync` |
| RC07 | Pedido (situação Pendente, `dt_pedido` UTC), itens e baixa de estoque no **mesmo `TransactionScope`**; `Complete()` só no final. | `PedidoService.FinalizarCompraAsync` |
| RC08 | A baixa de estoque é `qt_estoque = qt_estoque - @quantidade WHERE qt_estoque >= @quantidade`. Se nenhuma linha mudar (outra compra levou as unidades), o Service lança 111 e a transação é desfeita. | `JogoRepository.AtualizarEstoqueAsync` + `PedidoService` |
| RC09 | O mesmo jogo repetido no pedido é **consolidado** num único item (respeita o UNIQUE); a soma precisa ficar entre 1 e 10. | `FinalizarPedidoRequestValidator` + `PedidoService` |
| RC10 | "Meus pedidos" lista só os pedidos do usuário da sessão, do mais recente para o mais antigo (`ORDER BY dt_pedido DESC, co_seq_pedido DESC`), com situação e itens compostos por LINQ (3 consultas, qualquer que seja o número de pedidos). | `PedidoRepository.ListarPorUsuarioAsync` + `PedidoService.ListarMeusPedidosAsync` |
| RC11 | Um pedido de outro usuário não pode ser consultado: 113. | `PedidoService.BuscarMeuPedidoAsync` |
| RN03 (ajuste) | A exclusão lógica pode ser desfeita: o administrador reativa o jogo marcando "Ativo" no formulário de edição (`PUT` com `ativo: true`). | `FormularioJogo.jsx`, `JogoRepository.AlterarAsync` |

---

## 6. Telas

| Rota | Acesso | História |
| --- | --- | --- |
| `/cadastro` | Público (link em `/login`) | C01 — regra da senha visível antes do envio, erros por campo, entra logado |
| Menu (`Layout`) | — | C02 — cliente: Carrinho (badge) e Meus pedidos, sem Administração; visitante: Entrar, Cadastrar e atalho do carrinho com itens |
| `/jogos/:id` | Público | C03 — "Adicionar ao carrinho", bloqueado com aviso quando esgotado; confirmação visual |
| `/carrinho` | Logado | C03/C04 — alterar quantidade, remover com "Desfazer", resumo com total, "Finalizar compra" ("Finalizando...") |
| `/meus-pedidos` | Logado | C05 — badge da situação, data, total e itens |
| `/admin/jogos/:id/editar` | Administrador | Ajuste RN03 — switch "Ativo" para reativar |

---

## 7. Testes

### 7.1 Rastreabilidade

`dotnet test`: **40 testes, todos passando** (xUnit + Moq; nenhum acessa o banco).

| Teste | Regra / história |
| --- | --- |
| **PlataformaServiceTests** | |
| `BuscarPorId_IdInexistente_LancaNaoEncontrada` | 100 |
| `Inserir_NomeJaCadastrado_LancaNomeDuplicadoENaoGrava` | 101 (sem chamar `InserirAsync`) |
| `Inserir_RepositoryLancaRegistroDuplicado_LancaNomeDuplicado` | 101 (corrida no UNIQUE) |
| `Inserir_NomeLivre_DevolvePlataformaComIdGerado` | Inclusão com id gerado e textos normalizados |
| `Alterar_MantendoOProprioNome_AceitaEChamaAlterar` | 101 ignora o próprio id |
| `Excluir_PlataformaComJogos_LancaPossuiJogosENaoExclui` | 102 (sem chamar `ExcluirAsync`) |
| **JogoServiceTests** | |
| `Inserir_PlataformaInexistente_LancaPlataformaNaoEncontradaENaoGrava` | RN05 / 100 |
| `Inserir_CondicaoInexistente_LancaCondicaoNaoEncontradaENaoGrava` | RN05 / 104 |
| `Inserir_GeneroInexistente_LancaGeneroNaoEncontradoENaoGrava` | RN05 / 105 |
| `Listar_FiltrandoPorGenero_EnviaAoRepositorySoOsIdsDosJogosDoGenero` | Filtro por gênero com LINQ |
| `Listar_SemGenero_NaoConsultaVinculosENaoRestringeIds` | Filtro opcional |
| `Listar_PaginaComJogos_MontaPlataformaECondicaoComLinq` | Composição sem JOIN |
| `BuscarPorId_IdInexistente_LancaJogoNaoEncontrado` | 103 |
| `Desativar_IdInexistente_LancaJogoNaoEncontradoENaoDesativa` | RN03 / 103 |
| `BuscarPorId_JogoInativoParaVisitante_LancaJogoNaoEncontrado` | RN04 |
| `BuscarPorId_JogoInativoParaAdministrador_DevolveODetalhe` | RN04 |
| `Listar_VisitantePedindoInativos_RepositoryRecebeIncluirInativosFalse` | RN04 |
| `Listar_AdministradorComIncluirInativos_RepositoryRecebeIncluirInativosTrue` | RN04 |
| `Alterar_ComAtivoTrue_ReativaJogoDesativado` | RN03 (reativação) |
| **GeneroServiceTests** | |
| `Listar_DevolveOsGenerosDoRepository` | Listagem |
| `Listar_SemGeneros_DevolveListaVazia` | Listagem vazia |
| **CondicaoJogoServiceTests** | |
| `Listar_DevolveAsCondicoesComAsChavesFixas` | Domínio com chave fixa |
| **AutenticacaoServiceTests** | |
| `CadastrarCliente_EmailLivre_GravaClienteAtivoComSenhaEmHash` | C01 / RC01 |
| `CadastrarCliente_EmailJaCadastrado_LancaEmailJaCadastradoENaoGrava` | C01 / 106 |
| `CadastrarCliente_RepositoryLancaRegistroDuplicado_LancaEmailJaCadastrado` | C01 / 106 (corrida) |
| **PedidoServiceTests** | |
| `FinalizarCompra_ItensValidos_CalculaTotalPeloPrecoDoBancoEBaixaEstoque` | C04 / RC04, RC05, RC07 |
| `FinalizarCompra_MesmoJogoRepetido_ConsolidaNumUnicoItem` | C04 / RC09 |
| `FinalizarCompra_JogoInativo_LancaJogoIndisponivelSemGravar` | C04 / 110 |
| `FinalizarCompra_JogoInexistente_LancaJogoIndisponivelSemGravar` | C04 / 110 |
| `FinalizarCompra_QuantidadeAcimaDoEstoque_LancaEstoqueInsuficienteSemGravar` | C04 / 111 |
| `ListarMeusPedidos_ConsultaSoOUsuarioLogadoEMontaSituacaoEItensComLinq` | C05 / RC10 |
| `ListarMeusPedidos_SemPedidos_DevolveListaVaziaSemConsultarItens` | C05 |
| `BuscarMeuPedido_IdInexistente_LancaPedidoNaoEncontrado` | 109 |
| `BuscarMeuPedido_PedidoDeOutroUsuario_LancaPedidoDeOutroUsuario` | 113 / RC11 |
| **FinalizarPedidoRequestValidatorTests** | |
| `Validar_ItensValidos_NaoTemErros` | C04 / formato |
| `Validar_ListaVazia_TemErroEmItens` | C04 / 3 |
| `Validar_QuantidadeForaDaFaixa_TemErro` (0 e 11) | C04 / 3 |
| `Validar_JogoRepetidoSomandoMaisDeDez_TemErroDeQuantidadeTotal` | RC09 / 3 |
| `Validar_JogoRepetidoSomandoAteDez_EhValido` | RC09 |

### 7.2 Teste de rede de segurança (Aula 9, Etapa 4)

1. A checagem de nome duplicado em `PlataformaService.InserirAsync` foi comentada temporariamente.
2. `dotnet test`: **1 falha em 40**, `Inserir_NomeJaCadastrado_LancaNomeDuplicadoENaoGrava`. O teste detecta a quebra da regra.
3. A alteração foi desfeita (`git diff` vazio) e `dotnet test` voltou a **40/40**.

### 7.3 Teste de ponta a ponta (API real + MySQL 8.4)

| Passo | Esperado | Obtido |
| --- | --- | --- |
| Cadastro de cliente | 201 e sessão ativa | ✔ |
| E-mail repetido / senha curta | 409 (106) / 400 (3) | ✔ |
| Compra de 2 unidades (estoque 5) | 201, estoque 5 → 3, 1 pedido e 1 item gravados, total pelo preço do banco | ✔ |
| Compra acima do estoque | 409 (111), banco inalterado | ✔ |
| Jogo inativo / esgotado | 400 (110) / 409 (111) | ✔ |
| Quantidade 11 / lista vazia / jogo repetido somando 11 | 400 (3) | ✔ |
| Sem sessão | 401 (4) | ✔ |
| Jogo repetido somando 3 | 201 com um único item de 3 unidades | ✔ |
| Meus pedidos | 200, do mais recente para o mais antigo | ✔ |
| Reativação pelo administrador (`PUT` com `ativo: true`) | `st_ativo` 0 → 1 e jogo visível no catálogo; `DELETE` volta a 0 e 404 público | ✔ |
