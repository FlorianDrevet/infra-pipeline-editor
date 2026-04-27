---
description: 'Relire strictement le diff de la branche courante avant merge sur main'
mode: 'agent'
---

# Review pre-merge vers {{ targetBranch }}

- **Points d'attention optionnels** : {{ focusAreas }}

## Instructions

1. Utiliser `@review-expert` comme agent principal.
2. Si `{{ targetBranch }}` n'est pas renseignee, prendre `origin/main`, puis `main` en fallback.
3. Limiter la revue au diff destine au merge (`<base>...HEAD`), pas a l'ensemble du depot.
4. Prioriser `{{ focusAreas }}` si fourni, sans ignorer les risques critiques hors focus.
5. Sortie attendue : findings tries par severite, puis questions/hypotheses, puis backlog de correction pret a deleguer.
6. Ne corriger aucun fichier pendant cette execution : cette etape ne fait que la revue.