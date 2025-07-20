# NecroDraft Arena

Ein Top-down Autobattler mit modularem Minion-Crafting, entwickelt als Universitätsprototyp.

## Spielbeschreibung

NecroDraft Arena ist ein strategisches Autobattler-Spiel, in dem Spieler modulare Minions aus verschiedenen Körperteilen zusammenbauen und diese in automatisierten Kämpfen einsetzen. Das Spiel kombiniert Elemente des Deck-Building mit taktischem Placement und automatisiertem Combat.

## Download

**Spielbare Version:** Der aktuelle Build ist über [Releases](../../releases) verfügbar.

## Dokumentation

### Projektdokumentation
Die vollständige technische Dokumentation befindet sich im Ordner `ProjectDocumentation_LaTeX/` und enthält:
- LaTeX-Quelldateien für die Projektdokumentation
- Generierte PDF-Dokumentation
- Detaillierte technische Spezifikationen

### Technische Demonstration
Im Projektordner finden Sie:
- **Videos:** Technische Demonstrationsvideos
- **Screenshots:** Spielscreenshots und UI-Prototypen

## Entwicklung

### Technische Details
- **Engine:** Unity 6+ (2D Core Template)
- **Plattform:** PC (Windows)
- **Art Style:** Dark Pixel Art
- **Scope:** 20 Wellen, 1-5 Minions, Dynamische Partsystem

### Projektstruktur
```
Assets/
├── Scripts/          # C# Skripte
├── Scenes/           # Unity Szenen
├── Prefabs/          # Spielobjekte
├── ScriptableObjects/# Spieledaten
└── Art/              # Grafische Assets
```

## Kernmechaniken

- **Modulares Minioncrafting:** Kombiniere verschiedene Körperteile
- **Automatisierter Combat:** Strategisches Placement entscheidet über den Kampf
- **Progressive Wellen:** Steigende Schwierigkeit durch verschiedene Gegnertypen
- **Klassenbasierte Strategien:** Verschiedene Necromancerklassen mit einzigartigen Fähigkeiten (wip)

## Status

Dieses Projekt ist ein Hochschulprototyp, der die Kernspielmechaniken demonstriert. Der Fokus liegt auf der Implementierung des grundlegenden Gameplayloops und der modularen Minionerstellung.

## Links

- [Releases](../../releases) - Download der spielbaren Version
- [ProjectDocumentation_LaTeX](./ProjectDocumentation_LaTeX/) - Vollständige Projektdokumentation
- [GameDesignDocument](./GameDesignDocument/) - Spieldesign-Dokumentation

---

*Entwickelt als Hochschulprojekt - NecroDraft Arena*
