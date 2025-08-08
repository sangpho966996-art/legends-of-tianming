const express = require('express');
const db = require('../config/database');
const auth = require('../middleware/auth');
const router = express.Router();

router.get('/', async (req, res) => {
    try {
        const result = await db.query(
            'SELECT id, name, level, experience, created_at FROM guilds ORDER BY level DESC, experience DESC LIMIT 50'
        );

        res.json({ guilds: result.rows });
    } catch (error) {
        console.error('Get guilds error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.post('/', auth, async (req, res) => {
    try {
        const { name } = req.body;
        
        if (!name) {
            return res.status(400).json({ error: 'Guild name required' });
        }

        const existingGuild = await db.query(
            'SELECT id FROM guilds WHERE name = $1',
            [name]
        );

        if (existingGuild.rows.length > 0) {
            return res.status(409).json({ error: 'Guild name already exists' });
        }

        const result = await db.query(
            'INSERT INTO guilds (name, leader_id, level, experience, created_at) VALUES ($1, $2, $3, $4, NOW()) RETURNING *',
            [name, req.user.userId, 1, 0]
        );

        await db.query(
            'INSERT INTO guild_members (guild_id, character_id, rank, joined_at) VALUES ($1, $2, $3, NOW())',
            [result.rows[0].id, req.user.userId, 'Leader']
        );

        res.status(201).json({
            message: 'Guild created successfully',
            guild: result.rows[0]
        });
    } catch (error) {
        console.error('Create guild error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

module.exports = router;
