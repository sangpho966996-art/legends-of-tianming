const express = require('express');
const db = require('../config/database');
const auth = require('../middleware/auth');
const router = express.Router();

router.get('/market', async (req, res) => {
    try {
        const result = await db.query(`
            SELECT ml.id, ml.price, ml.created_at, i.name, i.type, i.rarity, i.stats
            FROM market_listings ml
            JOIN items i ON ml.item_id = i.id
            ORDER BY ml.created_at DESC
            LIMIT 50
        `);

        res.json({ listings: result.rows });
    } catch (error) {
        console.error('Get market error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.post('/trade', auth, async (req, res) => {
    try {
        const { targetPlayerId, offeredItems, requestedItems } = req.body;
        
        
        res.json({
            message: 'Trade initiated',
            tradeId: `trade_${Date.now()}`
        });
    } catch (error) {
        console.error('Trade error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

module.exports = router;
