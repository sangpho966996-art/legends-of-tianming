# Legends of Tianming - MMORPG

A martial arts MMORPG inspired by classic games like "Kiếm Thế" and "Võ Lâm Truyền Kỳ" but with completely original content, story, and characters.

## Project Overview

**Platforms**: PC (Windows) first, then Android  
**Graphics**: Isometric 2.5D view (35-45° camera angle)  
**Gameplay**: Real-time combat, grind-to-progress, PvE and PvP  
**Monetization**: Cosmetics and convenience only (no pay-to-win)  
**Tech Stack**: Unity (C#) + Node.js + PostgreSQL  

## Demo Features (MVP)

- **2 Character Classes**: Azure Cloud Sect (sword), Iron Bell Sect (staff)
- **1 Playable Region**: Qingze Plains (Lv. 30-50)
- **Combat System**: Real-time hitbox combat with skills and combos
- **PvE Content**: Monsters, quests, 5-player dungeon
- **PvP Mode**: 1v1 duels with ranking system
- **Multiplayer**: Support for 100+ players per server

## Character Classes

### Azure Cloud Sect
- **Weapon**: Single Sword
- **Playstyle**: Fast attacks, evasion, counterattacks
- **Key Skills**: Swift Strike, Cloud Step, Flowing Counter, Wind Blade

### Iron Bell Sect  
- **Weapon**: Staff/Polearm
- **Playstyle**: Crowd control, stuns, area damage
- **Key Skills**: Iron Sweep, Bell's Resonance, Staff Vault, Earth Shaker

## Development Timeline

- **Week 1-2**: Foundation & Setup
- **Week 3-4**: Character Systems
- **Week 5-6**: Combat Mechanics
- **Week 7-8**: World & PvE
- **Week 9-10**: PvP & Social
- **Week 11-12**: Polish & Deployment

## Repository Structure

```
legends-of-tianming/
├── client/                 # Unity project
├── server/                 # Node.js backend
├── database/              # PostgreSQL schemas
├── shared/                # Assets and documentation
└── deployment/           # Docker and deployment configs
```

## Getting Started

### Prerequisites
- Unity 2022.3 LTS
- Node.js 18+
- PostgreSQL 14+
- Git with LFS

### Setup Instructions
1. Clone the repository
2. Follow setup guides in each subdirectory
3. See `shared/docs/` for detailed documentation

## Performance Targets

- **60 FPS** on mid-range PC (GTX 1060, 8GB RAM)
- **100+ players** per server instance
- **<120ms latency** for combat actions
- **<5 second** loading times between areas

## Security Features

- Server-authoritative combat validation
- Anti-cheat detection and prevention
- Encrypted client-server communication
- Secure player data storage

## Contributing

This is a solo development project by Devin AI for user @sangpho966996-art.

## License

All rights reserved. Original content created to avoid copyright issues.

---

**Link to Devin run**: https://app.devin.ai/sessions/fbe56e12ef054f6a85989ee2b32960a4  
**Requested by**: @sangpho966996-art
