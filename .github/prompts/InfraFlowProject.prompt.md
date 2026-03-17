---
description: 'description'
---
# InfraFlow Project Prompt
Les questions que je vais te poser portent sur un projet qui vise à générer les fichiers bicep d'une infra azure et les pipelines en yml de azure Devops à partir d'une configuration que je stocke via une api dans une base de donnée.

Stack Technique dotnet 10, ef core, scalar, Aspire, Angular 19
J'ai une api qui sauvegarde la config en base de donéne postgres sql, une api qui genere le bicep, et un frontend Angular dans `src/Front`.

Pour l'authentification j'utilise external entra id. J'ai une app registration pour mes 2 apis. 

Mes Apis sont en CQRS et j'utilise MediatR pour la gestion des commandes et des requetes.
Le frontend consomme les APIs via Axios et centralise les URL dans `src/Front/src/environments/environment*.ts`.
