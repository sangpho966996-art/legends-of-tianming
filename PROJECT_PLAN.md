# Legends of Tianming - MMORPG Development Plan

## Project Overview
A martial arts MMORPG inspired by "Kiếm Thế" and "Võ Lâm Truyền Kỳ" with original content.

## Tech Stack Decision
- **Client**: Unity 2022.3 LTS (C#) with isometric 2.5D setup
- **Server**: Node.js with Express.js and Socket.io for real-time communication
- **Database**: PostgreSQL for persistent data storage
- **Networking**: WebSocket for real-time gameplay, REST API for non-critical operations
- **Deployment**: Docker containers for easy deployment

## 8-12 Week MVP Timeline

### Week 1-2: Foundation & Setup
**Epic 1: Project Infrastructure**
- [ ] Repository setup (client, server, shared)
- [ ] Unity project initialization with isometric camera
- [ ] Node.js server with basic WebSocket connection
- [ ] PostgreSQL database schema design
- [ ] Basic networking between client and server

### Week 3-4: Core Character Systems
**Epic 2: Character Foundation**
- [ ] Character creation system
- [ ] 2 Classes implementation (Azure Cloud Sect, Iron Bell Sect)
- [ ] Basic movement and camera controls
- [ ] Character stats and attributes system
- [ ] Equipment slots and inventory system

### Week 5-6: Combat System
**Epic 3: Combat Mechanics**
- [ ] Real-time combat with hitbox detection
- [ ] Skill system with cooldowns and rage meter
- [ ] Auto-target toggle functionality
- [ ] Status effects (stun, slow, knockup, silence)
- [ ] Basic skill trees (12 skills per class for demo)

### Week 7-8: World and PvE
**Epic 4: Game World**
- [ ] Qingze Plains region creation
- [ ] Monster spawning and AI
- [ ] Basic quest system (main + daily quests)
- [ ] Loot drops and material gathering
- [ ] One 5-player dungeon (Normal difficulty)

### Week 9-10: PvP and Social
**Epic 5: Multiplayer Features**
- [ ] 1v1 Duel system
- [ ] Basic guild system
- [ ] Player-to-player trading
- [ ] PvP ranking system
- [ ] Anti-cheat measures

### Week 11-12: Polish and Deployment
**Epic 6: Final Polish**
- [ ] UI/UX improvements
- [ ] Performance optimization
- [ ] Bug fixes and testing
- [ ] Server deployment
- [ ] Admin panel creation
- [ ] Documentation

## Folder Structure
```
legends-of-tianming/
├── client/                 # Unity project
│   ├── Assets/
│   │   ├── Scripts/
│   │   ├── Prefabs/
│   │   ├── Materials/
│   │   ├── Textures/
│   │   └── Scenes/
│   └── ProjectSettings/
├── server/                 # Node.js backend
│   ├── src/
│   │   ├── controllers/
│   │   ├── models/
│   │   ├── routes/
│   │   ├── services/
│   │   └── utils/
│   ├── config/
│   └── tests/
├── database/              # Database schemas and migrations
│   ├── migrations/
│   ├── seeds/
│   └── schemas/
├── shared/                # Shared assets and documentation
│   ├── assets/
│   ├── docs/
│   └── configs/
└── deployment/           # Docker and deployment configs
    ├── docker/
    └── scripts/
```

## Character Classes for Demo

### Azure Cloud Sect (Fast Swordplay)
- **Weapon**: Single Sword
- **Playstyle**: High mobility, evasion, counterattacks
- **Key Skills**: 
  - Swift Strike (basic attack combo)
  - Cloud Step (dash with invincibility frames)
  - Flowing Counter (parry and riposte)
  - Wind Blade (ranged sword energy)

### Iron Bell Sect (Staff Combat)
- **Weapon**: Staff/Polearm
- **Playstyle**: Crowd control, stuns, area damage
- **Key Skills**:
  - Iron Sweep (AoE knockdown)
  - Bell's Resonance (AoE stun)
  - Staff Vault (mobility skill)
  - Earth Shaker (ground slam AoE)

## Core Systems Priority

### High Priority (MVP)
1. Character movement and camera
2. Basic combat with 2 classes
3. Networking for multiplayer
4. One region with monsters
5. Basic PvP dueling
6. Equipment and inventory

### Medium Priority (Post-MVP)
1. Full skill trees (24 skills per class)
2. Guild system
3. Dungeons and world bosses
4. Advanced PvP modes
5. Crafting system

### Low Priority (Future Updates)
1. Additional regions
2. More character classes
3. Mounts system
4. Advanced graphics effects
5. Mobile optimization

## Technical Considerations

### Performance Targets
- 60 FPS on mid-range PC (GTX 1060, 8GB RAM)
- Support 100+ players per server instance
- <120ms latency for combat actions
- <5 second loading times between areas

### Security Measures
- Server-authoritative combat validation
- Encrypted client-server communication
- Anti-cheat detection for movement/combat
- Rate limiting for API calls
- Secure player data storage

### Scalability Plan
- Horizontal server scaling with load balancers
- Database sharding for player data
- CDN for asset delivery
- Regional server deployment (NA, SEA, EU)
