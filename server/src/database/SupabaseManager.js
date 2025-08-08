const { createClient } = require('@supabase/supabase-js');

class SupabaseManager {
    constructor() {
        this.supabase = null;
        this.isConnected = false;
        this.connectionRetries = 0;
        this.maxRetries = 3;
        this.retryDelay = 5000; // 5 seconds
        
        this.initializeConnection();
    }

    async initializeConnection() {
        try {
            const supabaseUrl = process.env.SUPABASE_URL;
            const supabaseKey = process.env.SUPABASE_ANON_KEY;
            
            if (!supabaseUrl || !supabaseKey) {
                console.error('❌ Supabase credentials not found in environment variables');
                return;
            }
            
            this.supabase = createClient(supabaseUrl, supabaseKey, {
                auth: {
                    autoRefreshToken: true,
                    persistSession: true
                },
                realtime: {
                    params: {
                        eventsPerSecond: 10
                    }
                }
            });
            
            await this.testConnection();
            
            console.log('✅ Supabase connection established successfully');
            this.isConnected = true;
            
            this.setupRealtimeSubscriptions();
            
        } catch (error) {
            console.error('❌ Failed to initialize Supabase connection:', error);
            await this.handleConnectionError();
        }
    }

    async testConnection() {
        try {
            const { data, error } = await this.supabase
                .from('players')
                .select('count')
                .limit(1);
            
            if (error) {
                throw error;
            }
            
            console.log('🔗 Supabase connection test successful');
            return true;
        } catch (error) {
            console.error('❌ Supabase connection test failed:', error);
            throw error;
        }
    }

    async handleConnectionError() {
        this.connectionRetries++;
        
        if (this.connectionRetries <= this.maxRetries) {
            console.log(`🔄 Retrying Supabase connection (${this.connectionRetries}/${this.maxRetries})...`);
            
            setTimeout(() => {
                this.initializeConnection();
            }, this.retryDelay);
        } else {
            console.error('❌ Max connection retries reached. Supabase connection failed.');
            this.isConnected = false;
        }
    }

    async createPlayer(playerData) {
        try {
            const { data, error } = await this.supabase
                .from('players')
                .insert([{
                    id: playerData.id,
                    username: playerData.username,
                    email: playerData.email,
                    created_at: new Date().toISOString(),
                    last_login: new Date().toISOString(),
                    is_online: true,
                    region: playerData.region || 'NA'
                }])
                .select();
            
            if (error) throw error;
            
            console.log(`👤 Player created: ${playerData.username}`);
            return data[0];
        } catch (error) {
            console.error('❌ Error creating player:', error);
            throw error;
        }
    }

    async getPlayer(playerId) {
        try {
            const { data, error } = await this.supabase
                .from('players')
                .select('*')
                .eq('id', playerId)
                .single();
            
            if (error) throw error;
            return data;
        } catch (error) {
            console.error(`❌ Error fetching player ${playerId}:`, error);
            return null;
        }
    }

    async updatePlayerStatus(playerId, isOnline, region = null) {
        try {
            const updateData = {
                is_online: isOnline,
                last_seen: new Date().toISOString()
            };
            
            if (region) {
                updateData.current_region = region;
            }
            
            if (isOnline) {
                updateData.last_login = new Date().toISOString();
            }
            
            const { data, error } = await this.supabase
                .from('players')
                .update(updateData)
                .eq('id', playerId)
                .select();
            
            if (error) throw error;
            
            console.log(`👤 Player ${playerId} status updated: ${isOnline ? 'online' : 'offline'}`);
            return data[0];
        } catch (error) {
            console.error(`❌ Error updating player status:`, error);
            throw error;
        }
    }

    async createCharacter(characterData) {
        try {
            const { data, error } = await this.supabase
                .from('characters')
                .insert([{
                    id: characterData.id,
                    player_id: characterData.playerId,
                    name: characterData.name,
                    class: characterData.class,
                    level: characterData.level || 1,
                    experience: characterData.experience || 0,
                    stats: characterData.stats || {},
                    position: characterData.position || { x: 0, y: 0, z: 0 },
                    current_region: characterData.region || 'qingze_plains',
                    created_at: new Date().toISOString()
                }])
                .select();
            
            if (error) throw error;
            
            console.log(`⚔️ Character created: ${characterData.name} (${characterData.class})`);
            return data[0];
        } catch (error) {
            console.error('❌ Error creating character:', error);
            throw error;
        }
    }

    async getCharacter(characterId) {
        try {
            const { data, error } = await this.supabase
                .from('characters')
                .select(`
                    *,
                    players (
                        username,
                        is_online
                    )
                `)
                .eq('id', characterId)
                .single();
            
            if (error) throw error;
            return data;
        } catch (error) {
            console.error(`❌ Error fetching character ${characterId}:`, error);
            return null;
        }
    }

    async updateCharacterPosition(characterId, position, region) {
        try {
            const { data, error } = await this.supabase
                .from('characters')
                .update({
                    position: position,
                    current_region: region,
                    last_updated: new Date().toISOString()
                })
                .eq('id', characterId)
                .select();
            
            if (error) throw error;
            return data[0];
        } catch (error) {
            console.error(`❌ Error updating character position:`, error);
            throw error;
        }
    }

    async updateCharacterStats(characterId, stats, level = null, experience = null) {
        try {
            const updateData = {
                stats: stats,
                last_updated: new Date().toISOString()
            };
            
            if (level !== null) updateData.level = level;
            if (experience !== null) updateData.experience = experience;
            
            const { data, error } = await this.supabase
                .from('characters')
                .update(updateData)
                .eq('id', characterId)
                .select();
            
            if (error) throw error;
            return data[0];
        } catch (error) {
            console.error(`❌ Error updating character stats:`, error);
            throw error;
        }
    }

    async createGuild(guildData) {
        try {
            const { data, error } = await this.supabase
                .from('guilds')
                .insert([{
                    id: guildData.id,
                    name: guildData.name,
                    leader_id: guildData.leaderId,
                    description: guildData.description || '',
                    level: 1,
                    experience: 0,
                    member_count: 1,
                    max_members: guildData.maxMembers || 50,
                    created_at: new Date().toISOString()
                }])
                .select();
            
            if (error) throw error;
            
            await this.addGuildMember(guildData.id, guildData.leaderId, 'leader');
            
            console.log(`🏰 Guild created: ${guildData.name}`);
            return data[0];
        } catch (error) {
            console.error('❌ Error creating guild:', error);
            throw error;
        }
    }

    async addGuildMember(guildId, playerId, rank = 'member') {
        try {
            const { data, error } = await this.supabase
                .from('guild_members')
                .insert([{
                    guild_id: guildId,
                    player_id: playerId,
                    rank: rank,
                    contribution: 0,
                    joined_at: new Date().toISOString()
                }])
                .select();
            
            if (error) throw error;
            
            await this.supabase
                .from('guilds')
                .update({
                    member_count: await this.getGuildMemberCount(guildId)
                })
                .eq('id', guildId);
            
            console.log(`🏰 Player ${playerId} joined guild ${guildId} as ${rank}`);
            return data[0];
        } catch (error) {
            console.error('❌ Error adding guild member:', error);
            throw error;
        }
    }

    async getGuildMemberCount(guildId) {
        try {
            const { count, error } = await this.supabase
                .from('guild_members')
                .select('*', { count: 'exact', head: true })
                .eq('guild_id', guildId);
            
            if (error) throw error;
            return count;
        } catch (error) {
            console.error('❌ Error getting guild member count:', error);
            return 0;
        }
    }

    async updatePvPRanking(playerId, rating, rank, wins, losses) {
        try {
            const { data, error } = await this.supabase
                .from('pvp_rankings')
                .upsert([{
                    player_id: playerId,
                    rating: rating,
                    rank: rank,
                    wins: wins,
                    losses: losses,
                    last_match: new Date().toISOString(),
                    season: this.getCurrentSeason()
                }])
                .select();
            
            if (error) throw error;
            
            console.log(`🏆 PvP ranking updated for player ${playerId}: ${rank} (${rating})`);
            return data[0];
        } catch (error) {
            console.error('❌ Error updating PvP ranking:', error);
            throw error;
        }
    }

    async getPvPLeaderboard(limit = 100) {
        try {
            const { data, error } = await this.supabase
                .from('pvp_rankings')
                .select(`
                    *,
                    players (
                        username
                    )
                `)
                .eq('season', this.getCurrentSeason())
                .order('rating', { ascending: false })
                .limit(limit);
            
            if (error) throw error;
            return data;
        } catch (error) {
            console.error('❌ Error fetching PvP leaderboard:', error);
            return [];
        }
    }

    setupRealtimeSubscriptions() {
        this.supabase
            .channel('player_status')
            .on('postgres_changes', {
                event: 'UPDATE',
                schema: 'public',
                table: 'players',
                filter: 'is_online=eq.true'
            }, (payload) => {
                this.handlePlayerStatusChange(payload);
            })
            .subscribe();

        this.supabase
            .channel('character_positions')
            .on('postgres_changes', {
                event: 'UPDATE',
                schema: 'public',
                table: 'characters'
            }, (payload) => {
                this.handleCharacterPositionUpdate(payload);
            })
            .subscribe();

        this.supabase
            .channel('guild_activities')
            .on('postgres_changes', {
                event: '*',
                schema: 'public',
                table: 'guild_members'
            }, (payload) => {
                this.handleGuildActivity(payload);
            })
            .subscribe();

        console.log('🔔 Real-time subscriptions established');
    }

    handlePlayerStatusChange(payload) {
        const { new: newRecord, old: oldRecord } = payload;
        
        if (newRecord.is_online !== oldRecord.is_online) {
            console.log(`👤 Player ${newRecord.username} is now ${newRecord.is_online ? 'online' : 'offline'}`);
            
            this.broadcastPlayerStatusChange(newRecord);
        }
    }

    handleCharacterPositionUpdate(payload) {
        const { new: newRecord } = payload;
        
        this.broadcastCharacterPosition(newRecord);
    }

    handleGuildActivity(payload) {
        const { eventType, new: newRecord } = payload;
        
        console.log(`🏰 Guild activity: ${eventType} for guild ${newRecord.guild_id}`);
        
        this.broadcastGuildActivity(newRecord.guild_id, eventType, newRecord);
    }

    broadcastPlayerStatusChange(playerData) {
        console.log('📡 Broadcasting player status change:', playerData.username);
    }

    broadcastCharacterPosition(characterData) {
        console.log('📡 Broadcasting character position:', characterData.name);
    }

    broadcastGuildActivity(guildId, eventType, data) {
        console.log(`📡 Broadcasting guild activity to guild ${guildId}:`, eventType);
    }

    getCurrentSeason() {
        const now = new Date();
        const seasonStart = new Date(now.getFullYear(), 0, 1); // January 1st
        const daysSinceStart = Math.floor((now - seasonStart) / (1000 * 60 * 60 * 24));
        return Math.floor(daysSinceStart / 90) + 1; // 90-day seasons
    }

    async executeQuery(query, params = []) {
        try {
            const { data, error } = await this.supabase.rpc(query, params);
            
            if (error) throw error;
            return data;
        } catch (error) {
            console.error('❌ Error executing query:', error);
            throw error;
        }
    }

    async healthCheck() {
        try {
            const { data, error } = await this.supabase
                .from('players')
                .select('count')
                .limit(1);
            
            return {
                status: error ? 'error' : 'healthy',
                connected: this.isConnected,
                error: error?.message || null,
                timestamp: new Date().toISOString()
            };
        } catch (error) {
            return {
                status: 'error',
                connected: false,
                error: error.message,
                timestamp: new Date().toISOString()
            };
        }
    }

    async disconnect() {
        if (this.supabase) {
            this.supabase.removeAllChannels();
            console.log('🔌 Supabase connection closed');
        }
        
        this.isConnected = false;
    }
}

module.exports = SupabaseManager;
