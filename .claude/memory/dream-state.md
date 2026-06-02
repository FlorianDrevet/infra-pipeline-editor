# Dream State

> Trace la date de la dernière consolidation mémoire (`/dream`).
> Déclenchement **manuel** : pas de gate, pas de compteur, pas de verrou.

| Champ | Valeur |
|-------|--------|
| `lastDreamDate` | 2026-05-29 |

## Règle

- La consolidation se lance à la main via la commande `/dream` quand la mémoire a grossi ou divergé.
- Le sous-agent `dream` met à jour `lastDreamDate` à la date du jour en fin de passe.
