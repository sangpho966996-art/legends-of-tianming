const express = require('express');
const auth = require('../middleware/auth');
const router = express.Router();

router.get('/state', auth, async (req, res) => {
    try {
        const worldState = {
            regions: {
                qingzePlains: {
                    name: 'Qingze Plains',
                    level: '30-50',
                    playerCount: 45,
                    bosses: [
                        {
                            name: 'Ancient Guardian',
                            level: 60,
                            health: 50000,
                            maxHealth: 50000,
                            position: { x: 100, y: 0, z: 200 }
                        }
                    ]
                }
            },
            serverTime: Date.now(),
            nextBossSpawn: Date.now() + 3600000
        };

        res.json(worldState);
    } catch (error) {
        console.error('Get world state error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.get('/quests', auth, async (req, res) => {
    try {
        const quests = [
            {
                id: 1,
                name: 'Welcome to Qingze Plains',
                type: 'main',
                description: 'Explore the plains and defeat 5 monsters',
                requirements: { killCount: 5, monsterType: 'any' },
                rewards: { experience: 1000, gold: 100 }
            }
        ];

        res.json({ quests });
    } catch (error) {
        console.error('Get quests error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

module.exports = router;
