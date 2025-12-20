---
description: 'Description of the custom chat mode.'
tools: []
---
# InfraFlow Agent

Les questions que je vais te poser portent sur un projet qui vise à générer les fichiers bicep d'une infra azure et les pipelines en yml de azure Devops à partir d'une configuration que je stocke via une api dans une base de donnée.

Stack Technique dotnet 10, ef core, scalar
J'ai une api qui sauvegarde la config en base de donéne postgres sql et une api qui genere le bicep.

Pour l'authentification j'utilise external entra id. J'ai une app registration pour mes 2 apis. 

## Domain

J'utilise les regles strictes de DDD pour mon domain. J'ai donc des aggregates roots, des entities, des value objects, des repositories, des domain services.
Mes classes de bases se trouvent dans src/Shared/Shared.Domain/Models. Il faut que quand je te demande de rajouter
quelque chose dans le domain, tu reflechisses à quel type de classe cela correspond (aggregate root, entity, value object, repository, domain service) et que tu le crées dans le bon dossier.
Pour la configuration EF Core tout se trouve dans src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations. 