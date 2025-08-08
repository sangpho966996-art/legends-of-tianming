const express = require('express');
const db = require('../config/database');
const auth = require('../middleware/auth');
const router = express.Router();

router.get('/characters', auth, async (req, res) => {
    try {
        const result = await db.query(
            'SELECT id, name, class, level, experience, stats, created_at FROM characters WHERE player_id = $1',
            [req.user.userId]
        );

        res.json({ characters: result.rows });
    } catch (error) {
        console.error('Get characters error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.post('/characters', auth, async (req, res) => {
    try {
        const { name, characterClass } = req.body;
        
        if (!name || !characterClass) {
            return res.status(400).json({ error: 'Name and class required' });
        }

        const validClasses = ['Azure Cloud Sect', 'Iron Bell Sect', 'Shadow Veil Sect', 'Wandering Spear Sect', 'Spirit Lute Sect', 'Stoneheart Sect'];
        if (!validClasses.includes(characterClass)) {
            return res.status(400).json({ error: 'Invalid character class' });
        }

        const existingChar = await db.query(
            'SELECT id FROM characters WHERE name = $1',
            [name]
        );

        if (existingChar.rows.length > 0) {
            return res.status(409).json({ error: 'Character name already exists' });
        }

        const baseStats = {
            'Azure Cloud Sect': { strength: 15, agility: 20, intelligence: 10, vitality: 15 },
            'Iron Bell Sect': { strength: 18, agility: 12, intelligence: 15, vitality: 15 },
            'Shadow Veil Sect': { strength: 12, agility: 22, intelligence: 12, vitality: 14 },
            'Wandering Spear Sect': { strength: 20, agility: 15, intelligence: 10, vitality: 15 },
            'Spirit Lute Sect': { strength: 10, agility: 15, intelligence: 20, vitality: 15 },
            'Stoneheart Sect': { strength: 22, agility: 8, intelligence: 12, vitality: 18 }
        };

        const result = await db.query(
            'INSERT INTO characters (player_id, name, class, level, experience, stats, created_at) VALUES ($1, $2, $3, $4, $5, $6, NOW()) RETURNING *',
            [req.user.userId, name, characterClass, 1, 0, JSON.stringify(baseStats[characterClass])]
        );

        res.status(201).json({
            message: 'Character created successfully',
            character: result.rows[0]
        });
    } catch (error) {
        console.error('Create character error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.get('/characters/:id', auth, async (req, res) => {
    try {
        const result = await db.query(
            'SELECT * FROM characters WHERE id = $1 AND player_id = $2',
            [req.params.id, req.user.userId]
        );

        if (result.rows.length === 0) {
            return res.status(404).json({ error: 'Character not found' });
        }

        const character = result.rows[0];
        
        const equipment = await db.query(
            'SELECT ce.slot, i.* FROM character_equipment ce JOIN items i ON ce.item_id = i.id WHERE ce.character_id = $1',
            [character.id]
        );

        const skills = await db.query(
            'SELECT skill_id, level, points_spent FROM character_skills WHERE character_id = $1',
            [character.id]
        );

        res.json({
            character: {
                ...character,
                equipment: equipment.rows,
                skills: skills.rows
            }
        });
    } catch (error) {
        console.error('Get character error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.put('/characters/:id/stats', auth, async (req, res) => {
    try {
        const { stats } = req.body;
        
        const result = await db.query(
            'UPDATE characters SET stats = $1 WHERE id = $2 AND player_id = $3 RETURNING *',
            [JSON.stringify(stats), req.params.id, req.user.userId]
        );

        if (result.rows.length === 0) {
            return res.status(404).json({ error: 'Character not found' });
        }

        res.json({
            message: 'Stats updated successfully',
            character: result.rows[0]
        });
    } catch (error) {
        console.error('Update stats error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

module.exports = router;
