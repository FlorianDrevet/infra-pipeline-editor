---
description: Review pré-merge stricte du diff de la branche courante (double passe merge-readiness + anti-vibe-coding). Ne corrige aucun fichier.
argument-hint: "[branche cible] [points d'attention]"
---

# Review pré-merge : $ARGUMENTS

## Instructions

1. Lancer le sous-agent `review-expert` (tool *Agent*) pour la passe merge-readiness principale.
2. Lancer ensuite `vibe-coding-refractaire` comme seconde passe (odeurs de vibe coding, abstractions bidon, code fragile).
3. Branche cible : si non renseignée, prendre `origin/main` puis `main` en fallback.
4. Limiter la revue au diff destiné au merge (`<base>...HEAD`), pas à tout le dépôt.
5. Prioriser les points d'attention fournis, sans ignorer les risques critiques hors focus.
6. Fusionner les doublons entre les deux passes en un finding plus fort.
7. Sortie : findings triés par sévérité → questions/hypothèses → backlog de correction prêt à déléguer.
8. **Ne corriger aucun fichier** : cette étape ne fait que la revue. (Pour appliquer le backlog ensuite : sous-agent `review-remediator`.)
