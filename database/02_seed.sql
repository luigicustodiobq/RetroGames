-- =====================================================================
-- RetroGames - 02_seed.sql
-- Dados de exemplo. Executar logo depois de 01_schema.sql, num banco
-- recém-criado.
--
--   * Condições e perfis com chaves fixas (iguais a CondicaoJogoIds e PerfilIds).
--   * 7 plataformas: "Atari 2600" fica propositalmente SEM jogos
--     (usada na contagem por plataforma da etapa 5).
--   * 9 gêneros e 24 jogos, cada jogo com 1 a 3 gêneros.
--   * Jogo inativo: "Mortal Kombat" (st_ativo = FALSE).
--   * Jogo com estoque zero: "Chrono Trigger" (qt_estoque = 0).
--   * tb_usuario NÃO recebe registros: o administrador é criado pela API.
-- =====================================================================

USE retrogames;

-- ---------------------------------------------------------------------
-- Condições de jogo (chave fixa)
-- ---------------------------------------------------------------------
INSERT INTO tb_condicao_jogo (co_condicao_jogo, ds_condicao_jogo) VALUES
    (1, 'Novo'),
    (2, 'Usado'),
    (3, 'Recondicionado');

-- ---------------------------------------------------------------------
-- Perfis (chave fixa)
-- ---------------------------------------------------------------------
INSERT INTO tb_perfil (co_perfil, ds_perfil) VALUES
    (1, 'Administrador'),
    (2, 'Cliente');

-- ---------------------------------------------------------------------
-- Plataformas
-- ---------------------------------------------------------------------
INSERT INTO tb_plataforma (co_seq_plataforma, no_plataforma, no_fabricante, nu_ano_lancamento) VALUES
    (1, 'Super Nintendo',   'Nintendo', 1990),
    (2, 'Mega Drive',       'Sega',     1988),
    (3, 'Nintendo 64',      'Nintendo', 1996),
    (4, 'PlayStation',      'Sony',     1994),
    (5, 'Game Boy',         'Nintendo', 1989),
    (6, 'NES',              'Nintendo', 1983),
    (7, 'Atari 2600',       'Atari',    1977);   -- sem jogos vinculados

-- ---------------------------------------------------------------------
-- Gêneros
-- ---------------------------------------------------------------------
INSERT INTO tb_genero (co_seq_genero, no_genero) VALUES
    (1, 'Plataforma'),
    (2, 'Ação'),
    (3, 'Aventura'),
    (4, 'RPG'),
    (5, 'Corrida'),
    (6, 'Luta'),
    (7, 'Tiro'),
    (8, 'Puzzle'),
    (9, 'Beat ''em up');

-- ---------------------------------------------------------------------
-- Jogos
-- co_condicao_jogo: 1 = Novo, 2 = Usado, 3 = Recondicionado
-- ---------------------------------------------------------------------
INSERT INTO tb_jogo
    (co_seq_jogo, no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque, st_ativo, co_plataforma, co_condicao_jogo)
VALUES
    -- Super Nintendo
    (1,  'Super Mario World', 'Mario e Yoshi atravessam Dinosaur Land para resgatar a princesa.', 1990, 249.90, 5, TRUE, 1, 2),
    (2,  'The Legend of Zelda: A Link to the Past', 'Link viaja entre o Mundo da Luz e o Mundo das Trevas.', 1991, 299.90, 3, TRUE, 1, 3),
    (3,  'Chrono Trigger', 'RPG de viagem no tempo com múltiplos finais.', 1995, 899.90, 0, TRUE, 1, 2),
    (4,  'Super Metroid', 'Samus Aran explora o planeta Zebes.', 1994, 349.90, 2, TRUE, 1, 2),
    (5,  'Donkey Kong Country', 'Donkey e Diddy Kong recuperam o estoque de bananas.', 1994, 199.90, 6, TRUE, 1, 3),
    (6,  'Street Fighter II Turbo', NULL, 1993, 179.90, 4, TRUE, 1, 2),
    -- Mega Drive
    (7,  'Sonic the Hedgehog', 'O ouriço azul enfrenta o Dr. Robotnik em alta velocidade.', 1991, 149.90, 8, TRUE, 2, 2),
    (8,  'Streets of Rage 2', 'Axel, Blaze, Max e Skate enfrentam o sindicato do Mr. X.', 1992, 229.90, 3, TRUE, 2, 3),
    (9,  'Mortal Kombat', 'Torneio de luta com golpes finais marcantes.', 1993, 159.90, 2, FALSE, 2, 2),
    (10, 'Phantasy Star IV', 'Conclusão da saga clássica de RPG da Sega.', 1993, 499.90, 1, TRUE, 2, 2),
    -- Nintendo 64
    (11, 'Super Mario 64', 'Mario estreia em ambientes 3D no castelo da princesa.', 1996, 279.90, 4, TRUE, 3, 2),
    (12, 'The Legend of Zelda: Ocarina of Time', 'Link viaja entre passado e futuro para deter Ganondorf.', 1998, 329.90, 3, TRUE, 3, 3),
    (13, 'Mario Kart 64', 'Corridas de kart com itens e até quatro jogadores.', 1996, 239.90, 5, TRUE, 3, 2),
    (14, 'GoldenEye 007', 'Tiro em primeira pessoa baseado no filme de James Bond.', 1997, 189.90, 2, TRUE, 3, 2),
    -- PlayStation
    (15, 'Final Fantasy VII', 'Cloud e a AVALANCHE enfrentam a Shinra e Sephiroth.', 1997, 259.90, 4, TRUE, 4, 2),
    (16, 'Crash Bandicoot', 'O marsupial enfrenta o Dr. Neo Cortex.', 1996, 129.90, 7, TRUE, 4, 1),
    (17, 'Gran Turismo', 'Simulador de corrida com centenas de carros reais.', 1997, 99.90, 6, TRUE, 4, 2),
    (18, 'Castlevania: Symphony of the Night', 'Alucard explora o castelo de Drácula.', 1997, 449.90, 1, TRUE, 4, 3),
    (19, 'Metal Gear Solid', 'Solid Snake se infiltra em Shadow Moses.', 1998, 219.90, 3, TRUE, 4, 2),
    -- Game Boy
    (20, 'Tetris', 'O clássico de encaixar peças que caem.', 1989, 89.90, 10, TRUE, 5, 2),
    (21, 'Pokémon Red', 'Capture e treine Pokémon na região de Kanto.', 1996, 349.90, 2, TRUE, 5, 3),
    (22, 'Super Mario Land', 'Mario viaja por Sarasaland para salvar Daisy.', 1989, 119.90, 5, TRUE, 5, 2),
    -- NES
    (23, 'Super Mario Bros.', 'A aventura original de Mario no Reino do Cogumelo.', 1985, 159.90, 6, TRUE, 6, 2),
    (24, 'Mega Man 2', 'Mega Man enfrenta os oito robôs do Dr. Wily.', 1988, 209.90, 2, TRUE, 6, 3);

-- ---------------------------------------------------------------------
-- Vínculos jogo x gênero (1 a 3 gêneros por jogo)
-- 1 Plataforma, 2 Ação, 3 Aventura, 4 RPG, 5 Corrida,
-- 6 Luta, 7 Tiro, 8 Puzzle, 9 Beat 'em up
-- ---------------------------------------------------------------------
INSERT INTO tb_jogo_genero (co_jogo, co_genero) VALUES
    (1, 1),
    (2, 2), (2, 3),
    (3, 4),
    (4, 1), (4, 2), (4, 3),
    (5, 1),
    (6, 6),
    (7, 1), (7, 2),
    (8, 2), (8, 9),
    (9, 6),
    (10, 4),
    (11, 1), (11, 3),
    (12, 2), (12, 3),
    (13, 5),
    (14, 7), (14, 2),
    (15, 4),
    (16, 1),
    (17, 5),
    (18, 2), (18, 3), (18, 4),
    (19, 2), (19, 3),
    (20, 8),
    (21, 4),
    (22, 1),
    (23, 1),
    (24, 1), (24, 2);
