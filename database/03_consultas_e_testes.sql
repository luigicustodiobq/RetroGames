-- =====================================================================
-- RetroGames - 03_consultas_e_testes.sql
-- Aula 1, Etapa 5: consultas do catálogo e testes de integridade.
--
-- Script de ESTUDO, executado no MySQL depois de 01_schema.sql e 02_seed.sql.
-- As consultas abaixo usam JOIN porque testam o banco diretamente; na camada
-- Repository da aplicação cada consulta lê uma única tabela e a composição é
-- feita no Service (Espec. 3.1).
-- =====================================================================

USE retrogames;

-- =====================================================================
-- PARTE 1 - CONSULTAS DO CATÁLOGO
-- =====================================================================

-- ---------------------------------------------------------------------
-- Consulta 1: catálogo - jogos ativos com o nome da plataforma e a
-- descrição da condição, ordenados por nome.
-- Resultado obtido: 23 jogos (os 24 do seed menos "Mortal Kombat", inativo).
-- ---------------------------------------------------------------------
SELECT j.co_seq_jogo,
       j.no_jogo,
       p.no_plataforma,
       c.ds_condicao_jogo,
       j.nu_ano_lancamento,
       j.vl_preco,
       j.qt_estoque
  FROM tb_jogo j
  INNER JOIN tb_plataforma    p ON p.co_seq_plataforma = j.co_plataforma
  INNER JOIN tb_condicao_jogo c ON c.co_condicao_jogo  = j.co_condicao_jogo
 WHERE j.st_ativo = TRUE
 ORDER BY j.no_jogo;

-- ---------------------------------------------------------------------
-- Consulta 2: gêneros de um jogo informado.
-- Resultado obtido para o jogo 18 (Castlevania: Symphony of the Night):
-- Ação, Aventura, RPG.
-- ---------------------------------------------------------------------
SET @idJogo = 18;

SELECT g.co_seq_genero,
       g.no_genero
  FROM tb_jogo_genero jg
  INNER JOIN tb_genero g ON g.co_seq_genero = jg.co_genero
 WHERE jg.co_jogo = @idJogo
 ORDER BY g.no_genero;

-- ---------------------------------------------------------------------
-- Consulta 3: jogos de um gênero informado.
-- Resultado obtido para o gênero 4 (RPG): Castlevania: Symphony of the
-- Night, Chrono Trigger, Final Fantasy VII, Phantasy Star IV, Pokémon Red.
-- ---------------------------------------------------------------------
SET @idGenero = 4;

SELECT j.co_seq_jogo,
       j.no_jogo,
       j.st_ativo
  FROM tb_jogo_genero jg
  INNER JOIN tb_jogo j ON j.co_seq_jogo = jg.co_jogo
 WHERE jg.co_genero = @idGenero
 ORDER BY j.no_jogo;

-- ---------------------------------------------------------------------
-- Consulta 4: quantidade de jogos por plataforma, incluindo plataformas
-- sem jogos (LEFT JOIN + COUNT da coluna do lado direito).
-- Resultado obtido: Atari 2600 aparece com 0.
-- ---------------------------------------------------------------------
SELECT p.co_seq_plataforma,
       p.no_plataforma,
       COUNT(j.co_seq_jogo) AS qt_jogos
  FROM tb_plataforma p
  LEFT JOIN tb_jogo j ON j.co_plataforma = p.co_seq_plataforma
 GROUP BY p.co_seq_plataforma, p.no_plataforma
 ORDER BY p.no_plataforma;

-- ---------------------------------------------------------------------
-- Consulta 5: busca por parte do nome do jogo, 5 itens por página.
-- Com o termo 'er' existem 7 jogos ativos: a página 1 traz 5 e a
-- página 2 traz os 2 restantes.
-- OFFSET = (pagina - 1) * tamanhoPagina
-- ---------------------------------------------------------------------
SET @busca = 'er';

-- Total encontrado (mesmo filtro da listagem). Resultado obtido: 7.
SELECT COUNT(*) AS total
  FROM tb_jogo
 WHERE st_ativo = TRUE
   AND no_jogo LIKE CONCAT('%', @busca, '%');

-- Página 1
SELECT co_seq_jogo, no_jogo
  FROM tb_jogo
 WHERE st_ativo = TRUE
   AND no_jogo LIKE CONCAT('%', @busca, '%')
 ORDER BY no_jogo
 LIMIT 5 OFFSET 0;

-- Página 2
SELECT co_seq_jogo, no_jogo
  FROM tb_jogo
 WHERE st_ativo = TRUE
   AND no_jogo LIKE CONCAT('%', @busca, '%')
 ORDER BY no_jogo
 LIMIT 5 OFFSET 5;


-- =====================================================================
-- PARTE 2 - TESTES DE INTEGRIDADE
-- Cada instrução foi executada no MySQL 8.4 local; o resultado real está
-- anotado no comentário. Os testes 1 a 4 DEVEM falhar.
-- =====================================================================

-- Teste 1: jogo com co_plataforma inexistente.
-- Esperado: erro 1452 (chave estrangeira).
-- Obtido:   ERROR 1452 (23000): Cannot add or update a child row: a foreign
--           key constraint fails (... CONSTRAINT `fk_jogo_plataforma` ...)
INSERT INTO tb_jogo (no_jogo, nu_ano_lancamento, vl_preco, qt_estoque, co_plataforma, co_condicao_jogo)
VALUES ('Teste FK', 1990, 100.00, 1, 999, 1);

-- Teste 2: plataforma com nome já cadastrado.
-- Esperado: erro 1062 (UNIQUE).
-- Obtido:   ERROR 1062 (23000): Duplicate entry 'Super Nintendo' for key
--           'tb_plataforma.uk_plataforma_no'
INSERT INTO tb_plataforma (no_plataforma, no_fabricante, nu_ano_lancamento)
VALUES ('Super Nintendo', 'Nintendo', 1990);

-- Teste 3: excluir plataforma que possui jogos.
-- Esperado: erro 1451 (chave estrangeira).
-- Obtido:   ERROR 1451 (23000): Cannot delete or update a parent row: a
--           foreign key constraint fails (... CONSTRAINT `fk_jogo_plataforma` ...)
DELETE FROM tb_plataforma WHERE co_seq_plataforma = 1;

-- Teste 4: jogo com no_jogo nulo.
-- Esperado: erro 1048 (NOT NULL).
-- Obtido:   ERROR 1048 (23000): Column 'no_jogo' cannot be null
INSERT INTO tb_jogo (no_jogo, nu_ano_lancamento, vl_preco, qt_estoque, co_plataforma, co_condicao_jogo)
VALUES (NULL, 1990, 100.00, 1, 1, 1);

-- Teste 5: jogo com vl_preco igual a 0.
-- Esperado: aceito - o banco não tem CHECK; a regra RN06 fica no Service.
-- Obtido:   1 row affected (registro gravado com vl_preco = 0.00).
INSERT INTO tb_jogo (no_jogo, nu_ano_lancamento, vl_preco, qt_estoque, co_plataforma, co_condicao_jogo)
VALUES ('Teste Preco Zero', 1990, 0.00, 1, 1, 1);


-- =====================================================================
-- PARTE 3 - LIMPEZA DOS DADOS DE TESTE
-- Só o teste 5 gravou dados. Ele não tem vínculo em tb_jogo_genero,
-- então pode ser apagado diretamente.
-- Obtido: 1 row affected; depois disso tb_jogo voltou a ter 24 registros.
-- Observação: as tentativas que falharam consomem valores do
-- AUTO_INCREMENT (lacunas na numeração), o que não deixa dado residual.
-- =====================================================================
DELETE FROM tb_jogo WHERE no_jogo = 'Teste Preco Zero';

-- Conferência: deve devolver 0.
SELECT COUNT(*) AS dados_de_teste
  FROM tb_jogo
 WHERE no_jogo IN ('Teste FK', 'Teste Preco Zero') OR no_jogo IS NULL;
