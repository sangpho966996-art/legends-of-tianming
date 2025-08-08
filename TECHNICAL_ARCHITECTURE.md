# Technical Architecture - Legends of Tianming

## System Architecture Overview

```
[Unity Client] <--WebSocket--> [Node.js Server] <---> [PostgreSQL DB]
     |                              |
     |                              |
[Local Assets]                 [Game Logic]
[UI Systems]                   [Anti-cheat]
[Rendering]                    [Player Data]
```

## Client Architecture (Unity)

### Core Managers
- **GameManager**: Main game state controller
- **NetworkManager**: Client-server communication
- **PlayerManager**: Local player state and input
- **CombatManager**: Combat calculations and effects
- **UIManager**: User interface coordination
- **AudioManager**: Sound effects and music
- **SceneManager**: Level loading and transitions

### Key Systems
- **Character Controller**: Movement, jumping, dashing
- **Combat System**: Hitboxes, damage calculation, status effects
- **Skill System**: Cooldowns, combos, skill trees
- **Inventory System**: Equipment, items, trading
- **Camera System**: Isometric follow camera with smooth transitions
- **Input System**: Keyboard/mouse with gamepad support planned

### Networking Layer
- **WebSocket Client**: Real-time communication for combat/movement
- **HTTP Client**: REST API calls for non-critical operations
- **State Synchronization**: Player positions, health, skills
- **Lag Compensation**: Client-side prediction with server reconciliation

## Server Architecture (Node.js)

### Core Services
- **AuthService**: Player authentication and session management
- **GameService**: Core game logic and state management
- **CombatService**: Server-authoritative combat validation
- **PlayerService**: Player data and character management
- **GuildService**: Guild operations and social features
- **EconomyService**: Trading, auction house, item management

### API Structure
```
/api/v1/
├── auth/           # Authentication endpoints
├── players/        # Player data and characters
├── combat/         # Combat validation
├── guilds/         # Guild management
├── economy/        # Trading and items
├── world/          # World state and NPCs
└── admin/          # Admin panel endpoints
```

### Real-time Events (WebSocket)
- Player movement and position updates
- Combat actions and damage
- Skill usage and cooldowns
- Chat messages
- Guild notifications
- World events (boss spawns, etc.)

## Database Schema (PostgreSQL)

### Core Tables
```sql
-- Players and Characters
players (id, username, email, password_hash, created_at)
characters (id, player_id, name, class, level, experience, stats)
character_equipment (character_id, slot, item_id)
character_skills (character_id, skill_id, level, points_spent)

-- Items and Economy
items (id, name, type, rarity, stats, description)
player_inventory (character_id, item_id, quantity, slot)
market_listings (id, seller_id, item_id, price, created_at)

-- Guilds and Social
guilds (id, name, leader_id, level, experience, created_at)
guild_members (guild_id, character_id, rank, joined_at)
guild_upgrades (guild_id, upgrade_type, level)

-- World and Quests
quests (id, name, type, requirements, rewards, description)
character_quests (character_id, quest_id, status, progress)
world_state (region, boss_spawn_time, events)

-- PvP and Rankings
pvp_matches (id, player1_id, player2_id, winner_id, match_type, timestamp)
rankings (character_id, rating, wins, losses, season)
```

## Security Implementation

### Client Security
- Code obfuscation for release builds
- Asset encryption for critical game data
- Input validation before sending to server
- Anti-tampering measures for game files

### Server Security
- JWT tokens for authentication
- Rate limiting on all endpoints
- Input sanitization and validation
- SQL injection prevention
- DDoS protection with rate limiting

### Anti-cheat Measures
- Server-authoritative movement validation
- Combat action verification
- Impossible action detection (speed hacks, teleporting)
- Statistical analysis for suspicious behavior
- Regular client integrity checks

## Performance Optimization

### Client Optimization
- Object pooling for frequently spawned objects
- LOD (Level of Detail) for distant characters
- Texture streaming for large worlds
- Efficient UI rendering with Canvas groups
- Audio compression and streaming

### Server Optimization
- Connection pooling for database
- Caching frequently accessed data (Redis)
- Efficient data structures for game state
- Batch processing for non-critical updates
- Load balancing across multiple instances

### Network Optimization
- Delta compression for state updates
- Priority-based packet sending
- Lag compensation techniques
- Efficient serialization (MessagePack)
- Regional server deployment

## Development Tools and Workflow

### Version Control
- Git with LFS for large assets
- Branching strategy: main/develop/feature branches
- Automated testing on pull requests
- Code review requirements

### Build Pipeline
- Automated Unity builds for multiple platforms
- Docker containerization for server deployment
- Database migration scripts
- Asset optimization pipeline

### Monitoring and Analytics
- Server performance monitoring
- Player behavior analytics
- Error tracking and logging
- Real-time player count and server health

## Deployment Strategy

### Development Environment
- Local Unity editor for client development
- Local Node.js server with hot reload
- Local PostgreSQL database
- Docker Compose for easy setup

### Staging Environment
- Cloud-hosted staging server
- Automated deployment from develop branch
- Integration testing environment
- Performance testing setup

### Production Environment
- Multi-region deployment (NA, SEA, EU)
- Load balancers and auto-scaling
- Database replication and backups
- CDN for asset delivery
- Monitoring and alerting systems

## Scalability Considerations

### Horizontal Scaling
- Stateless server design for easy scaling
- Database sharding by player regions
- Microservices architecture for different game systems
- Message queues for inter-service communication

### Performance Targets
- Support 500+ concurrent players per server instance
- <100ms response time for combat actions
- 99.9% uptime for production servers
- <5 second loading times for world transitions
