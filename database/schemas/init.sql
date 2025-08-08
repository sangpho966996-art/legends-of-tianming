


CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE players (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_login TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT TRUE,
    is_banned BOOLEAN DEFAULT FALSE,
    ban_reason TEXT,
    ban_expires_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE characters (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    name VARCHAR(50) UNIQUE NOT NULL,
    class VARCHAR(50) NOT NULL CHECK (class IN ('Azure Cloud Sect', 'Iron Bell Sect', 'Shadow Veil Sect', 'Wandering Spear Sect', 'Spirit Lute Sect', 'Stoneheart Sect')),
    level INTEGER DEFAULT 1 CHECK (level >= 1 AND level <= 100),
    experience BIGINT DEFAULT 0,
    stats JSONB NOT NULL DEFAULT '{"strength": 10, "agility": 10, "intelligence": 10, "vitality": 10}',
    position JSONB DEFAULT '{"x": 0, "y": 0, "z": 0, "region": "qingzePlains"}',
    health INTEGER DEFAULT 100,
    max_health INTEGER DEFAULT 100,
    mana INTEGER DEFAULT 100,
    max_mana INTEGER DEFAULT 100,
    gold INTEGER DEFAULT 1000,
    silver INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_played TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL CHECK (type IN ('weapon', 'armor', 'accessory', 'consumable', 'material', 'gem')),
    subtype VARCHAR(50), -- sword, staff, helmet, boots, etc.
    rarity VARCHAR(20) NOT NULL CHECK (rarity IN ('common', 'uncommon', 'rare', 'epic')),
    level_requirement INTEGER DEFAULT 1,
    class_requirement VARCHAR(50), -- NULL means any class can use
    stats JSONB DEFAULT '{}', -- stat bonuses
    effects JSONB DEFAULT '[]', -- special effects
    description TEXT,
    icon_path VARCHAR(255),
    max_stack INTEGER DEFAULT 1,
    is_tradeable BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE player_inventory (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id),
    quantity INTEGER DEFAULT 1 CHECK (quantity > 0),
    slot INTEGER, -- inventory slot position
    gem_sockets JSONB DEFAULT '[]', -- socketed gems
    upgrade_level INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(character_id, slot)
);

CREATE TABLE character_equipment (
    character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
    slot VARCHAR(20) NOT NULL CHECK (slot IN ('weapon', 'helmet', 'chest', 'legs', 'boots', 'gloves', 'ring1', 'ring2', 'necklace')),
    item_id UUID NOT NULL REFERENCES items(id),
    gem_sockets JSONB DEFAULT '[]',
    upgrade_level INTEGER DEFAULT 0,
    equipped_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (character_id, slot)
);

CREATE TABLE skills (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    class VARCHAR(50) NOT NULL,
    skill_tree_position JSONB NOT NULL, -- {x: 0, y: 0, tier: 1}
    max_level INTEGER DEFAULT 10,
    prerequisites JSONB DEFAULT '[]', -- required skill IDs
    description TEXT,
    effects JSONB NOT NULL, -- damage, cooldown, mana cost, etc.
    animation_name VARCHAR(100),
    icon_path VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE character_skills (
    character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
    skill_id UUID NOT NULL REFERENCES skills(id),
    level INTEGER DEFAULT 1 CHECK (level >= 1),
    points_spent INTEGER DEFAULT 1,
    learned_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (character_id, skill_id)
);

CREATE TABLE guilds (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) UNIQUE NOT NULL,
    leader_id UUID NOT NULL REFERENCES characters(id),
    level INTEGER DEFAULT 1 CHECK (level >= 1 AND level <= 50),
    experience BIGINT DEFAULT 0,
    description TEXT,
    max_members INTEGER DEFAULT 50,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE guild_members (
    guild_id UUID NOT NULL REFERENCES guilds(id) ON DELETE CASCADE,
    character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
    rank VARCHAR(20) DEFAULT 'Member' CHECK (rank IN ('Leader', 'Officer', 'Member')),
    contribution_points INTEGER DEFAULT 0,
    joined_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (guild_id, character_id)
);

CREATE TABLE guild_upgrades (
    guild_id UUID NOT NULL REFERENCES guilds(id) ON DELETE CASCADE,
    upgrade_type VARCHAR(50) NOT NULL,
    level INTEGER DEFAULT 1,
    upgraded_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (guild_id, upgrade_type)
);

CREATE TABLE quests (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(200) NOT NULL,
    type VARCHAR(20) NOT NULL CHECK (type IN ('main', 'daily', 'weekly', 'guild')),
    level_requirement INTEGER DEFAULT 1,
    region VARCHAR(50),
    description TEXT NOT NULL,
    objectives JSONB NOT NULL, -- [{type: 'kill', target: 'monster_id', count: 5}]
    rewards JSONB NOT NULL, -- {experience: 1000, gold: 100, items: []}
    prerequisites JSONB DEFAULT '[]', -- required quest IDs
    is_repeatable BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE character_quests (
    character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
    quest_id UUID NOT NULL REFERENCES quests(id),
    status VARCHAR(20) DEFAULT 'active' CHECK (status IN ('active', 'completed', 'failed')),
    progress JSONB DEFAULT '{}', -- objective progress
    started_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    completed_at TIMESTAMP WITH TIME ZONE,
    PRIMARY KEY (character_id, quest_id)
);

CREATE TABLE market_listings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    seller_id UUID NOT NULL REFERENCES characters(id),
    item_id UUID NOT NULL REFERENCES items(id),
    quantity INTEGER DEFAULT 1,
    price INTEGER NOT NULL CHECK (price > 0),
    gem_sockets JSONB DEFAULT '[]',
    upgrade_level INTEGER DEFAULT 0,
    expires_at TIMESTAMP WITH TIME ZONE DEFAULT (NOW() + INTERVAL '7 days'),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE pvp_matches (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    match_type VARCHAR(20) NOT NULL CHECK (match_type IN ('duel', 'battleground', 'siege')),
    player1_id UUID NOT NULL REFERENCES characters(id),
    player2_id UUID REFERENCES characters(id), -- NULL for team matches
    team1_ids JSONB DEFAULT '[]', -- for team matches
    team2_ids JSONB DEFAULT '[]',
    winner_id UUID REFERENCES characters(id),
    winner_team INTEGER, -- 1 or 2 for team matches
    duration_seconds INTEGER,
    match_data JSONB DEFAULT '{}', -- detailed match statistics
    started_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    ended_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE rankings (
    character_id UUID PRIMARY KEY REFERENCES characters(id) ON DELETE CASCADE,
    rating INTEGER DEFAULT 1000,
    wins INTEGER DEFAULT 0,
    losses INTEGER DEFAULT 0,
    season INTEGER DEFAULT 1,
    peak_rating INTEGER DEFAULT 1000,
    last_match_at TIMESTAMP WITH TIME ZONE,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE world_state (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    region VARCHAR(50) NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    event_data JSONB NOT NULL,
    started_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE monsters (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL, -- 'monster', 'boss', 'npc'
    level INTEGER NOT NULL,
    region VARCHAR(50) NOT NULL,
    spawn_position JSONB NOT NULL,
    stats JSONB NOT NULL,
    loot_table JSONB DEFAULT '[]',
    respawn_time INTEGER DEFAULT 30, -- seconds
    ai_behavior VARCHAR(50) DEFAULT 'aggressive',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_characters_player_id ON characters(player_id);
CREATE INDEX idx_characters_name ON characters(name);
CREATE INDEX idx_characters_level ON characters(level);
CREATE INDEX idx_player_inventory_character_id ON player_inventory(character_id);
CREATE INDEX idx_character_equipment_character_id ON character_equipment(character_id);
CREATE INDEX idx_character_skills_character_id ON character_skills(character_id);
CREATE INDEX idx_guild_members_guild_id ON guild_members(guild_id);
CREATE INDEX idx_guild_members_character_id ON guild_members(character_id);
CREATE INDEX idx_character_quests_character_id ON character_quests(character_id);
CREATE INDEX idx_market_listings_seller_id ON market_listings(seller_id);
CREATE INDEX idx_market_listings_expires_at ON market_listings(expires_at);
CREATE INDEX idx_pvp_matches_player1_id ON pvp_matches(player1_id);
CREATE INDEX idx_pvp_matches_player2_id ON pvp_matches(player2_id);
CREATE INDEX idx_rankings_rating ON rankings(rating DESC);
CREATE INDEX idx_world_state_region ON world_state(region);
CREATE INDEX idx_monsters_region ON monsters(region);

CREATE OR REPLACE FUNCTION update_character_last_played()
RETURNS TRIGGER AS $$
BEGIN
    NEW.last_played = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_character_last_played
    BEFORE UPDATE ON characters
    FOR EACH ROW
    EXECUTE FUNCTION update_character_last_played();

CREATE OR REPLACE FUNCTION calculate_character_stats(char_id UUID)
RETURNS JSONB AS $$
DECLARE
    base_stats JSONB;
    equipment_stats JSONB := '{"strength": 0, "agility": 0, "intelligence": 0, "vitality": 0}';
    final_stats JSONB;
    equipment_record RECORD;
BEGIN
    SELECT stats INTO base_stats FROM characters WHERE id = char_id;
    
    FOR equipment_record IN 
        SELECT i.stats FROM character_equipment ce
        JOIN items i ON ce.item_id = i.id
        WHERE ce.character_id = char_id
    LOOP
        equipment_stats := equipment_stats || equipment_record.stats;
    END LOOP;
    
    final_stats := jsonb_build_object(
        'strength', COALESCE((base_stats->>'strength')::int, 0) + COALESCE((equipment_stats->>'strength')::int, 0),
        'agility', COALESCE((base_stats->>'agility')::int, 0) + COALESCE((equipment_stats->>'agility')::int, 0),
        'intelligence', COALESCE((base_stats->>'intelligence')::int, 0) + COALESCE((equipment_stats->>'intelligence')::int, 0),
        'vitality', COALESCE((base_stats->>'vitality')::int, 0) + COALESCE((equipment_stats->>'vitality')::int, 0)
    );
    
    RETURN final_stats;
END;
$$ LANGUAGE plpgsql;
