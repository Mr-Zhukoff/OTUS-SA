﻿CREATE DATABASE users
    WITH
    OWNER = pguser
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;



CREATE TABLE "users" (
  "id" SERIAL PRIMARY KEY,
  "firstname" VARCHAR(100) NOT NULL,
  "lastname" VARCHAR(100) NOT NULL,
  "middlename" VARCHAR(100),
  "email" VARCHAR(255) NOT NULL,
  "salt" VARCHAR(255),
  "hash" VARCHAR(255)
);