CREATE DATABASE users
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;


CREATE TABLE "users" (
  "id" SERIAL PRIMARY KEY,
  "firstname" VARCHAR(100) NOT NULL,
  "lastname" VARCHAR(100) NOT NULL,
  "middlename" VARCHAR(100) NOT NULL,
  "email" VARCHAR(255) NOT NULL
);