-- =====================================================================
-- RetroGames - 04_modulo_compras.sql
-- Atualiza um banco JÁ EXISTENTE (Aulas 1 a 6) com o Módulo de Compras,
-- sem apagar nem alterar dados: usuários (inclusive o administrador),
-- jogos e plataformas continuam como estão.
--
-- Pode ser executado mais de uma vez (idempotente):
--   * CREATE TABLE IF NOT EXISTS não recria tabelas existentes;
--   * INSERT IGNORE não duplica as situações de chave fixa.
--
-- Em um banco novo, basta rodar 01_schema.sql e 02_seed.sql, que já
-- contêm estas tabelas.
-- =====================================================================

USE retrogames;

CREATE TABLE IF NOT EXISTS tb_situacao_pedido (
    co_situacao_pedido  INT          NOT NULL,
    ds_situacao_pedido  VARCHAR(50)  NOT NULL,
    CONSTRAINT pk_situacao_pedido PRIMARY KEY (co_situacao_pedido),
    CONSTRAINT uk_situacao_pedido_ds UNIQUE (ds_situacao_pedido)
);

CREATE TABLE IF NOT EXISTS tb_pedido (
    co_seq_pedido       INT            NOT NULL AUTO_INCREMENT,
    dt_pedido           DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    vl_total            DECIMAL(10,2)  NOT NULL,
    co_usuario          INT            NOT NULL,
    co_situacao_pedido  INT            NOT NULL,
    CONSTRAINT pk_pedido PRIMARY KEY (co_seq_pedido),
    INDEX ix_pedido_usuario (co_usuario),
    CONSTRAINT fk_pedido_usuario
        FOREIGN KEY (co_usuario) REFERENCES tb_usuario (co_seq_usuario),
    CONSTRAINT fk_pedido_situacao_pedido
        FOREIGN KEY (co_situacao_pedido) REFERENCES tb_situacao_pedido (co_situacao_pedido)
);

CREATE TABLE IF NOT EXISTS tb_item_pedido (
    co_seq_item_pedido  INT            NOT NULL AUTO_INCREMENT,
    qt_item             INT            NOT NULL,
    vl_unitario         DECIMAL(10,2)  NOT NULL,
    co_pedido           INT            NOT NULL,
    co_jogo             INT            NOT NULL,
    CONSTRAINT pk_item_pedido PRIMARY KEY (co_seq_item_pedido),
    CONSTRAINT uk_item_pedido_pedido_jogo UNIQUE (co_pedido, co_jogo),
    CONSTRAINT fk_item_pedido_pedido
        FOREIGN KEY (co_pedido) REFERENCES tb_pedido (co_seq_pedido),
    CONSTRAINT fk_item_pedido_jogo
        FOREIGN KEY (co_jogo) REFERENCES tb_jogo (co_seq_jogo)
);

INSERT IGNORE INTO tb_situacao_pedido (co_situacao_pedido, ds_situacao_pedido) VALUES
    (1, 'Pendente'),
    (2, 'Pago'),
    (3, 'Enviado'),
    (4, 'Cancelado');

-- Conferência
SHOW TABLES;
SELECT co_situacao_pedido, ds_situacao_pedido FROM tb_situacao_pedido ORDER BY co_situacao_pedido;
