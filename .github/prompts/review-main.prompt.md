---
description: 'Relire strictement le diff de la branche courante avant merge sur main avec une double passe merge-readiness et anti-vibe coding'
mode: 'agent'
---

# Review pre-merge vers {{ targetBranch }}

- **Points d'attention optionnels** : {{ focusAreas }}

## Instructions

1. Utiliser `@review-expert` pour la passe merge-readiness principale.
2. Executer ensuite `@vibe-coding-refractaire` comme seconde passe pour traquer les odeurs de vibe coding, les abstractions bidon et le code fragile qui passe trop facilement sous le radar.
3. Si `{{ targetBranch }}` n'est pas renseignee, prendre `origin/main`, puis `main` en fallback.
4. Limiter la revue au diff destine au merge (`<base>...HEAD`), pas a l'ensemble du depot.
5. Prioriser `{{ focusAreas }}` si fourni, sans ignorer les risques critiques hors focus.
6. Fusionner les doublons entre les deux passes en un seul finding plus fort quand elles pointent le meme defaut.
7. Sortie attendue : findings tries par severite, puis questions/hypotheses, puis backlog de correction pret a deleguer.
8. Ne corriger aucun fichier pendant cette execution : cette etape ne fait que la revue.