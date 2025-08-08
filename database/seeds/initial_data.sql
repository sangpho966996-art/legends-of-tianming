
INSERT INTO skills (id, name, class, skill_tree_position, max_level, description, effects) VALUES
(uuid_generate_v4(), 'Swift Strike', 'Azure Cloud Sect', '{"x": 0, "y": 0, "tier": 1}', 10, 'A quick sword strike that can be chained into combos', '{"damage": 120, "cooldown": 2000, "mana_cost": 10, "combo_window": 1500}'),
(uuid_generate_v4(), 'Cloud Step', 'Azure Cloud Sect', '{"x": 1, "y": 0, "tier": 1}', 10, 'Dash forward with brief invincibility frames', '{"distance": 8, "cooldown": 8000, "mana_cost": 20, "invincibility_duration": 500}'),
(uuid_generate_v4(), 'Flowing Counter', 'Azure Cloud Sect', '{"x": 0, "y": 1, "tier": 2}', 10, 'Parry an attack and counter with increased damage', '{"damage_multiplier": 1.5, "cooldown": 12000, "mana_cost": 25, "parry_window": 800}'),
(uuid_generate_v4(), 'Wind Blade', 'Azure Cloud Sect', '{"x": 1, "y": 1, "tier": 2}', 10, 'Launch a ranged sword energy projectile', '{"damage": 180, "range": 15, "cooldown": 6000, "mana_cost": 30}'),
(uuid_generate_v4(), 'Sword Dance', 'Azure Cloud Sect', '{"x": 2, "y": 0, "tier": 2}', 10, 'Perform a spinning attack hitting all nearby enemies', '{"damage": 150, "radius": 5, "cooldown": 15000, "mana_cost": 40}'),
(uuid_generate_v4(), 'Azure Storm', 'Azure Cloud Sect', '{"x": 1, "y": 2, "tier": 3}', 5, 'Ultimate ability: Multiple sword strikes in rapid succession', '{"hits": 7, "damage_per_hit": 100, "cooldown": 45000, "mana_cost": 80}');

INSERT INTO skills (id, name, class, skill_tree_position, max_level, description, effects) VALUES
(uuid_generate_v4(), 'Iron Sweep', 'Iron Bell Sect', '{"x": 0, "y": 0, "tier": 1}', 10, 'Sweep attack that knocks down enemies in an arc', '{"damage": 140, "arc_angle": 120, "cooldown": 4000, "mana_cost": 15, "knockdown_duration": 2000}'),
(uuid_generate_v4(), 'Bell''s Resonance', 'Iron Bell Sect', '{"x": 1, "y": 0, "tier": 1}', 10, 'AoE stun that affects all nearby enemies', '{"radius": 8, "stun_duration": 3000, "cooldown": 20000, "mana_cost": 35}'),
(uuid_generate_v4(), 'Staff Vault', 'Iron Bell Sect', '{"x": 0, "y": 1, "tier": 2}', 10, 'Pole vault over enemies, landing with impact damage', '{"damage": 160, "distance": 10, "cooldown": 10000, "mana_cost": 25}'),
(uuid_generate_v4(), 'Earth Shaker', 'Iron Bell Sect', '{"x": 1, "y": 1, "tier": 2}', 10, 'Ground slam that creates damaging shockwaves', '{"damage": 200, "radius": 12, "cooldown": 8000, "mana_cost": 40}'),
(uuid_generate_v4(), 'Iron Fortress', 'Iron Bell Sect', '{"x": 2, "y": 0, "tier": 2}', 10, 'Defensive stance that reduces incoming damage', '{"damage_reduction": 0.5, "duration": 8000, "cooldown": 25000, "mana_cost": 30}'),
(uuid_generate_v4(), 'Mountain Crusher', 'Iron Bell Sect', '{"x": 1, "y": 2, "tier": 3}', 5, 'Ultimate ability: Massive overhead slam with huge AoE', '{"damage": 400, "radius": 15, "cooldown": 60000, "mana_cost": 100}');

INSERT INTO items (id, name, type, subtype, rarity, level_requirement, class_requirement, stats, description) VALUES
(uuid_generate_v4(), 'Novice Sword', 'weapon', 'sword', 'common', 1, 'Azure Cloud Sect', '{"strength": 5, "agility": 2}', 'A basic training sword for new disciples'),
(uuid_generate_v4(), 'Cloud Piercer', 'weapon', 'sword', 'uncommon', 10, 'Azure Cloud Sect', '{"strength": 12, "agility": 8}', 'A swift blade favored by Azure Cloud disciples'),
(uuid_generate_v4(), 'Wind Cutter', 'weapon', 'sword', 'rare', 25, 'Azure Cloud Sect', '{"strength": 25, "agility": 18}', 'An elegant sword that seems to cut through air itself'),
(uuid_generate_v4(), 'Azure Dragon Blade', 'weapon', 'sword', 'epic', 40, 'Azure Cloud Sect', '{"strength": 45, "agility": 35}', 'A legendary sword imbued with the power of the Azure Dragon'),

(uuid_generate_v4(), 'Training Staff', 'weapon', 'staff', 'common', 1, 'Iron Bell Sect', '{"strength": 6, "vitality": 3}', 'A sturdy wooden staff for training'),
(uuid_generate_v4(), 'Iron Rod', 'weapon', 'staff', 'uncommon', 10, 'Iron Bell Sect', '{"strength": 15, "vitality": 10}', 'A heavy iron staff that rings like a bell when struck'),
(uuid_generate_v4(), 'Thunder Staff', 'weapon', 'staff', 'rare', 25, 'Iron Bell Sect', '{"strength": 30, "vitality": 20}', 'A staff that crackles with electrical energy'),
(uuid_generate_v4(), 'Mountain Breaker', 'weapon', 'staff', 'epic', 40, 'Iron Bell Sect', '{"strength": 50, "vitality": 30}', 'A massive staff capable of shattering mountains'),

(uuid_generate_v4(), 'Cloth Robes', 'armor', 'chest', 'common', 1, NULL, '{"vitality": 5}', 'Simple cloth robes worn by novice martial artists'),
(uuid_generate_v4(), 'Leather Vest', 'armor', 'chest', 'uncommon', 10, NULL, '{"vitality": 12, "agility": 3}', 'Flexible leather armor that doesn''t restrict movement'),
(uuid_generate_v4(), 'Reinforced Tunic', 'armor', 'chest', 'rare', 25, NULL, '{"vitality": 25, "strength": 5}', 'A tunic reinforced with metal plates'),
(uuid_generate_v4(), 'Dragon Scale Armor', 'armor', 'chest', 'epic', 40, NULL, '{"vitality": 45, "strength": 10, "agility": 5}', 'Armor crafted from ancient dragon scales'),

(uuid_generate_v4(), 'Jade Ring', 'accessory', 'ring', 'uncommon', 5, NULL, '{"intelligence": 8}', 'A ring carved from pure jade that enhances mental clarity'),
(uuid_generate_v4(), 'Tiger Claw Necklace', 'accessory', 'necklace', 'rare', 20, NULL, '{"strength": 15, "agility": 10}', 'A necklace made from the claw of a fierce tiger'),
(uuid_generate_v4(), 'Phoenix Feather Pendant', 'accessory', 'necklace', 'epic', 35, NULL, '{"intelligence": 25, "vitality": 15}', 'A pendant containing a feather from the legendary Phoenix');

INSERT INTO quests (id, name, type, level_requirement, region, description, objectives, rewards) VALUES
(uuid_generate_v4(), 'Welcome to Qingze Plains', 'main', 1, 'qingzePlains', 'Explore the plains and prove your worth by defeating monsters', '[{"type": "kill", "target": "any", "count": 5}]', '{"experience": 1000, "gold": 100}'),
(uuid_generate_v4(), 'Gather Herbs', 'daily', 5, 'qingzePlains', 'Collect medicinal herbs for the village healer', '[{"type": "gather", "target": "healing_herb", "count": 10}]', '{"experience": 500, "gold": 50, "silver": 100}'),
(uuid_generate_v4(), 'Bandit Threat', 'main', 10, 'qingzePlains', 'Eliminate the bandit leader threatening local merchants', '[{"type": "kill", "target": "bandit_leader", "count": 1}]', '{"experience": 2500, "gold": 300, "items": ["uncommon_weapon_box"]}'),
(uuid_generate_v4(), 'Ancient Ruins', 'main', 20, 'qingzePlains', 'Investigate the mysterious ruins discovered by farmers', '[{"type": "explore", "target": "ancient_ruins", "count": 1}]', '{"experience": 5000, "gold": 500}'),
(uuid_generate_v4(), 'Daily Training', 'daily', 1, 'qingzePlains', 'Complete your daily martial arts training', '[{"type": "skill_use", "target": "any", "count": 20}]', '{"experience": 300, "silver": 50}');

INSERT INTO monsters (id, name, type, level, region, spawn_position, stats, loot_table, respawn_time) VALUES
(uuid_generate_v4(), 'Wild Boar', 'monster', 32, 'qingzePlains', '{"x": 50, "y": 0, "z": 100}', '{"health": 800, "damage": 45, "defense": 10}', '[{"item": "boar_hide", "chance": 0.6}, {"item": "raw_meat", "chance": 0.8}]', 30),
(uuid_generate_v4(), 'Forest Wolf', 'monster', 35, 'qingzePlains', '{"x": -30, "y": 0, "z": 80}', '{"health": 600, "damage": 60, "defense": 5, "speed": 1.2}', '[{"item": "wolf_pelt", "chance": 0.5}, {"item": "sharp_fang", "chance": 0.3}]', 45),
(uuid_generate_v4(), 'Stone Golem', 'monster', 40, 'qingzePlains', '{"x": 0, "y": 0, "z": 200}', '{"health": 1500, "damage": 80, "defense": 25, "speed": 0.7}', '[{"item": "stone_core", "chance": 0.4}, {"item": "earth_essence", "chance": 0.2}]', 120),
(uuid_generate_v4(), 'Bandit Scout', 'monster', 38, 'qingzePlains', '{"x": 120, "y": 0, "z": 60}', '{"health": 700, "damage": 70, "defense": 15}', '[{"item": "bandit_coin", "chance": 0.7}, {"item": "rusty_weapon", "chance": 0.3}]', 60),
(uuid_generate_v4(), 'Ancient Guardian', 'boss', 60, 'qingzePlains', '{"x": 0, "y": 0, "z": 0}', '{"health": 50000, "damage": 200, "defense": 50}', '[{"item": "guardian_crystal", "chance": 1.0}, {"item": "epic_weapon_fragment", "chance": 0.1}]', 7200);

INSERT INTO world_state (region, event_type, event_data, expires_at) VALUES
('qingzePlains', 'boss_spawn', '{"boss_id": "ancient_guardian", "position": {"x": 0, "y": 0, "z": 0}}', NOW() + INTERVAL '2 hours'),
('qingzePlains', 'double_exp', '{"multiplier": 2.0, "description": "Double experience event in Qingze Plains"}', NOW() + INTERVAL '24 hours');

INSERT INTO players (username, email, password_hash) VALUES
('admin', 'admin@tianming.game', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj3bp.Gm.F5e');

INSERT INTO characters (player_id, name, class, level, experience, stats, position) VALUES
((SELECT id FROM players WHERE username = 'admin'), 'TestHero', 'Azure Cloud Sect', 30, 45000, '{"strength": 25, "agility": 30, "intelligence": 15, "vitality": 20}', '{"x": 0, "y": 0, "z": 0, "region": "qingzePlains"}');
