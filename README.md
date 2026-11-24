# RetroGames

Loja on-line de jogos retrô: catálogo público, área de administração e **Módulo de Compras** (Desafio, Versão Básica C01 a C05).

| Camada | Tecnologia |
| --- | --- |
| API | .NET 10 (ASP.NET Core com controllers), ADO.NET + MySqlConnector, FluentValidation |
| Banco | MySQL 8.4 |
| Testes | xUnit + Moq |
| Front-end | React + Vite + React Router + Bootstrap + Axios |

Arquitetura em camadas: **Controller → Service → Repository**. O Repository lê **uma tabela por consulta** (sem `JOIN`); a composição dos dados é feita no Service com LINQ. Transações (`TransactionScope`) são abertas só no Service. Todas as respostas usam o envelope `{ responseCode, errorMessage, data }`.

---

## Como executar

```bash
# 1. Banco (MySQL 8.4)
mysql -u root -p < database/01_schema.sql
mysql -u root -p < database/02_seed.sql

# 2. API (http://localhost:5080, Swagger em /swagger)
cp .env.example .env        # preencha a senha do MySQL e do administrador inicial
dotnet run --project RetroGames.WebApi --launch-profile http

# 3. Front-end (http://localhost:5173, com proxy /api -> 5080)
cd retrogames.web
npm install
npm run dev

# Testes unitários
dotnet test
```

O administrador inicial é criado pela API na primeira execução, com os dados de `AdminInicial__Email` e `AdminInicial__Senha` do `.env` (nunca versionado). Clientes se cadastram pela tela `/cadastro`.

---

## Banco de dados — Módulo de Compras

| Tabela | Colunas | Observações |
| --- | --- | --- |
| `tb_situacao_pedido` | `co_situacao_pedido` INT PK, `ds_situacao_pedido` VARCHAR(50) UNIQUE | Chave fixa, sem `AUTO_INCREMENT`: 1 Pendente, 2 Pago, 3 Enviado, 4 Cancelado (`SituacaoPedidoIds`) |
| `tb_pedido` | `co_seq_pedido` PK AI, `dt_pedido` DATETIME (UTC), `vl_total` DECIMAL(10,2), `co_usuario` FK, `co_situacao_pedido` FK | `vl_total` calculado no Service com os preços do banco |
| `tb_item_pedido` | `co_seq_item_pedido` PK AI, `qt_item`, `vl_unitario` DECIMAL(10,2), `co_pedido` FK, `co_jogo` FK | `UNIQUE (co_pedido, co_jogo)`; `vl_unitario` guarda o preço pago na compra |

Sem triggers, functions, `CHECK` ou `ON DELETE CASCADE/SET NULL`: as regras ficam no Service, onde são testadas.

- **Banco novo:** `01_schema.sql` + `02_seed.sql` já incluem as três tabelas e as situações (validados num banco vazio).
- **Banco existente (Aulas 1 a 6):** rode `database/04_modulo_compras.sql`. Ele é idempotente (`CREATE TABLE IF NOT EXISTS` + `INSERT IGNORE`) e não toca em usuários (o administrador continua), jogos ou plataformas.

Relacionamentos: `tb_usuario 1 — N tb_pedido`, `tb_situacao_pedido 1 — N tb_pedido`, `tb_pedido 1 — N tb_item_pedido`, `tb_jogo 1 — N tb_item_pedido`.

---

## Endpoints

| Método | Rota | Acesso | Descrição |
| --- | --- | --- | --- |
| GET | `/api/Jogo` | Público | Catálogo paginado (`busca`, `idPlataforma`, `idGenero`, `pagina`, `tamanhoPagina`, `incluirInativos`*) |
| GET | `/api/Jogo/{id}` | Público | Detalhe do jogo |
| POST/PUT/DELETE | `/api/Jogo[/{id}]` | Administrador | Inclusão, alteração e exclusão lógica |
| GET/POST/PUT/DELETE | `/api/Plataforma[/{id}]` | GET público; gravação Administrador | Manutenção de plataformas |
| GET | `/api/Genero`, `/api/CondicaoJogo` | Público | Tabelas de domínio |
| POST | `/api/Autenticacao/login` · `/logout` · GET `/eu` | — | Sessão por cookie HttpOnly `retrogames.sessao` |
| **POST** | **`/api/Autenticacao/cadastrar`** | Público | **C01** — cadastra cliente e inicia a sessão (HTTP 201) |
| **POST** | **`/api/Pedido`** | Logado | **C04** — finaliza a compra (HTTP 201 + `Location`) |
| **GET** | **`/api/Pedido/meus`** | Logado | **C05** — pedidos do usuário logado, do mais recente ao mais antigo |
| GET | `/api/Pedido/{id}` | Logado | Um pedido do próprio usuário (usado no `Location` do POST) |

\* `incluirInativos` só tem efeito para administrador (RN04).

Exemplo — `POST /api/Pedido` (o usuário vem da sessão e o preço vem do banco; o front-end envia só jogo e quantidade):

```json
{ "itens": [ { "idJogo": 1, "quantidade": 2 } ] }
```

```json
{
  "responseCode": 0, "errorMessage": null,
  "data": {
    "id": 1, "dataPedido": "2026-11-23T23:58:16Z", "valorTotal": 499.80,
    "idSituacaoPedido": 1, "descricaoSituacao": "Pendente",
    "itens": [ { "idJogo": 1, "nomeJogo": "Super Mario World", "quantidade": 2, "valorUnitario": 249.90, "subtotal": 499.80 } ]
  }
}
```

---

## Regras de negócio e códigos de resposta

Códigos gerais (`ResponseCodes`): 0 sucesso · 1 erro interno (500) · 2 corpo vazio (400) · 3 validação por campo (400, erros em `data`) · 4 não autenticado (401) · 5 acesso negado (403) · 6 credenciais inválidas (401).

Erros de negócio (`BusinessErrors`):

| Código | HTTP | Constante | Mensagem | Onde |
| --- | --- | --- | --- | --- |
| 100 | 404 | `PLATAFORMA_NAO_ENCONTRADA` | Plataforma não encontrada. | Plataforma / Jogo |
| 101 | 409 | `PLATAFORMA_NOME_DUPLICADO` | Já existe uma plataforma com este nome. | Plataforma |
| 102 | 409 | `PLATAFORMA_POSSUI_JOGOS` | Não é possível excluir: existem jogos desta plataforma. | Plataforma |
| 103 | 404 | `JOGO_NAO_ENCONTRADO` | Jogo não encontrado. | Jogo |
| 104 | 404 | `CONDICAO_NAO_ENCONTRADA` | A condição informada não existe. | Jogo |
| 105 | 404 | `GENERO_NAO_ENCONTRADO` | Um ou mais gêneros informados não existem. | Jogo |
| **106** | **409** | `EMAIL_JA_CADASTRADO` | Já existe uma conta com este e-mail. | **C01** |
| **109** | **404** | `PEDIDO_NAO_ENCONTRADO` | Pedido não encontrado. | Pedido |
| **110** | **400** | `JOGO_INDISPONIVEL` | Um ou mais jogos não estão disponíveis para venda. | **C04** (jogo inexistente ou inativo) |
| **111** | **409** | `ESTOQUE_INSUFICIENTE` | Estoque insuficiente. | **C04** |
| **113** | **403** | `PEDIDO_DE_OUTRO_USUARIO` | Este pedido pertence a outro usuário. | Pedido |

Os códigos 107, 108 e 112 pertencem às versões estendidas do desafio e não fazem parte da Versão Básica.

Regras do Módulo de Compras:

- **Preço sempre do banco:** o front-end nunca envia preço; `vl_unitario` e `vl_total` são calculados no `PedidoService`.
- **Usuário sempre da sessão:** `IdUsuario` é lido de `IUsuarioAtualService` (cookie), nunca do corpo.
- **Transação única:** pedido, itens e baixa de estoque ficam no mesmo `TransactionScope`; `Complete()` só no final. Se qualquer passo falhar, nada é gravado.
- **Estoque nunca negativo:** além da conferência no Service, o `UPDATE` de baixa só altera a linha se `qt_estoque >= quantidade`; se outra compra levar as últimas unidades no meio, o Service lança 111 e a transação é desfeita.
- **Itens consolidados:** o mesmo jogo enviado duas vezes vira um único item (respeita o `UNIQUE (co_pedido, co_jogo)`); o validator exige que a soma de cada jogo fique entre 1 e 10 (400 / código 3).
- **Reativação de jogo:** no formulário de edição, o administrador marca "Ativo" e o `PUT /api/Jogo/{id}` grava `st_ativo = TRUE`, desfazendo a exclusão lógica.
- **Carrinho no navegador:** `localStorage` (`retrogames_carrinho`) guarda só os itens; a sessão continua no cookie HttpOnly (RS01). Quantidade por jogo entre 1 e `min(10, estoque)`.

---

## Rastreabilidade de testes

`dotnet test` — **40 testes, todos passando**. A tabela completa, teste a teste, está em [`docs/ESPECIFICACAO.md`, seção 7.1](docs/ESPECIFICACAO.md#71-rastreabilidade).

| Arquivo | Testes | Cobre |
| --- | --- | --- |
| `PlataformaServiceTests` | 6 | 100, 101 (verificação e `RegistroDuplicadoException`), inclusão com id gerado, alteração mantendo o nome, 102 |
| `JogoServiceTests` | 13 | RN05 (100, 104, 105 sem gravar), filtro por gênero com LINQ, listagem sem gênero, composição de plataforma/condição/gêneros, 103 na busca e na exclusão lógica (RN03), **RN04** (visitante não vê inativo; administrador com `incluirInativos` vê) e **reativação** com `Ativo = true` |
| `GeneroServiceTests` | 2 | Listagem e lista vazia |
| `CondicaoJogoServiceTests` | 1 | Listagem com as chaves fixas |
| `AutenticacaoServiceTests` | 3 | **C01**: cadastro com perfil Cliente e senha em hash; 106 na verificação e na corrida do UNIQUE |
| `PedidoServiceTests` | 9 | **C04**: total pelo preço do banco + baixa de estoque, consolidação de itens, 110 (inativo e inexistente) e 111 sem gravar · **C05**: só pedidos do usuário, ordenação e composição com LINQ, lista vazia · 109 e 113 |
| `FinalizarPedidoRequestValidatorTests` | 6 | Lista vazia, quantidade fora de 1–10, **jogo repetido cuja soma passa de 10** (400 / código 3) |

**Rede de segurança (Aula 9, Etapa 4):** com a checagem de nome duplicado de `PlataformaService` comentada, `dotnet test` acusou 1 falha (`Inserir_NomeJaCadastrado_LancaNomeDuplicadoENaoGrava`); desfeita a alteração, voltou a 40/40.

Teste de ponta a ponta executado contra a API real (5080) e o MySQL 8.4 local:

| Passo | Resultado esperado | Obtido |
| --- | --- | --- |
| Cadastro de cliente | HTTP 201, sessão iniciada (`/eu` responde 200) | ✔ |
| Cadastro com e-mail repetido / senha curta | 409 (106) / 400 (3) | ✔ |
| Compra de 2 unidades (estoque 5) | HTTP 201, estoque 5 → 3, 1 linha em `tb_pedido` e em `tb_item_pedido`, total 2 × preço do banco | ✔ |
| Compra acima do estoque | HTTP 409 (111), estoque e contagens inalterados | ✔ |
| Jogo inativo / esgotado / quantidade 11 / lista vazia / sem sessão | 400 (110) / 409 (111) / 400 (3) / 400 (3) / 401 (4) | ✔ |
| Mesmo jogo repetido somando 11 / somando 3 | 400 (3) / 201 com **um** item de 3 unidades | ✔ |
| Dois pedidos seguidos em Meus pedidos | O mais recente primeiro | ✔ |
| Administrador reativa jogo inativo (`PUT` com `ativo: true`) | `st_ativo` 0 → 1 e jogo volta ao catálogo público | ✔ |
| Meus pedidos | HTTP 200 com o pedido, situação "Pendente" e itens com preço pago | ✔ |

---

## Front-end — telas

| Rota | Acesso | Tela |
| --- | --- | --- |
| `/` | Público | Catálogo com busca, filtros na URL e paginação |
| `/jogos/:id` | Público | Detalhe do jogo e "Adicionar ao carrinho" |
| `/login`, `/cadastro` | Público | Entrar e criar conta (C01) |
| `/carrinho` | Logado | Carrinho e finalização da compra (C03, C04) |
| `/meus-pedidos` | Logado | Pedidos do cliente (C05) |
| `/admin/jogos`, `/admin/jogos/novo`, `/admin/jogos/:id/editar` | Administrador | Manutenção de jogos |
| `/admin/plataformas` | Administrador | Manutenção de plataformas |

Identidade visual própria (fundo tinta-escura, magenta "fliperama" e ciano "fósforo", tipografia Space Grotesk + Inter), contraste AA, foco visível por teclado, link "Pular para o conteúdo", botões com área de toque de 44 px e layout testado para 360 px (celular) e 1366 px (desktop).

### Heurísticas de Nielsen nas telas do RetroGames

| # | Heurística | Onde aparece |
| --- | --- | --- |
| 1 | Visibilidade do status do sistema | Spinners com `role="status"` em todas as listas; "Entrando...", "Criando conta...", "Salvando...", **"Finalizando..."** com botão desabilitado; aviso flutuante ao adicionar ao carrinho; badge com a quantidade do carrinho no menu; título da aba muda por página; mensagem "Compra concluída! Pedido nº X" em `/meus-pedidos` |
| 2 | Correspondência com o mundo real | Preços em R$ (`R$ 149,90`), datas `dd/mm/aaaa HH:mm`, situações com explicação ("Aguardando a confirmação do pagamento"), termos de loja (Carrinho, Meus pedidos, Esgotado) |
| 3 | Controle e liberdade do usuário | **"Desfazer"** ao remover item do carrinho; Cancelar na edição de plataforma e no formulário de jogo; "Limpar filtros"; trilha de navegação e "Voltar ao catálogo"; login volta para a página de origem (`voltarPara`) |
| 4 | Consistência e padrões | Mesmo envelope de erro em toda a API; mesmos componentes `Carregando` / `MensagemErro` / `EstadoVazio`; botões primário (magenta) e secundário (contorno) com o mesmo papel em todas as telas |
| 5 | Prevenção de erros | "Adicionar ao carrinho" **bloqueado com aviso** quando o jogo está esgotado; quantidade limitada a `min(10, estoque)`; confirmação antes de excluir plataforma e desativar jogo; "Desativar" desabilitado para jogo já inativo; preço nunca enviado pelo front-end |
| 6 | Reconhecer em vez de lembrar | **Resumo do pedido** sempre visível no carrinho; filtros ativos mantidos na URL e nos campos; formulário de edição já preenchido; "Você já tem N no carrinho" no detalhe |
| 7 | Flexibilidade e eficiência | Filtros compartilháveis pela URL; botões − / + e digitação direta da quantidade; atalho do carrinho no menu; navegação completa por teclado |
| 8 | Estética e design minimalista | Cards com só o essencial (nome, plataforma, ano, condição, gêneros, preço); hierarquia tipográfica clara; uma ação primária por tela |
| 9 | Ajudar a reconhecer, diagnosticar e corrigir erros | Erros por campo (`is-invalid` + `invalid-feedback`) vindos do `responseCode` 3; mensagens da API em português ("Estoque insuficiente."); falha na compra explica o que fazer ("Ajuste as quantidades ou remova o item"); botão "Tentar novamente" |
| 10 | Ajuda e documentação | **Regra da senha exibida antes do envio** no cadastro, com contagem do que falta; dicas nos campos ("Campos com * são obrigatórios", "até 10 unidades por jogo"); estados vazios que dizem o próximo passo |
