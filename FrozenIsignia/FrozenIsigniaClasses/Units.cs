using System;
using System.Collections.Generic;

namespace FrozenIsigniaClasses
{
    public enum Weapon { Sword, Axe, Lance, Anima, Light, Dark, Bow, Staff }
    public enum Mount { None, Horse, Pegasus, Wyvern }
    public enum MoveGroup { Foot, Armor, Knight, Horse, Fighter, Bandit, Pirate, Mage, Flier }

    public class Unit
    {
        private static Random rand = new Random();

        public static String[] types = new String[] { "Lord(Sword)", "Lord(Axe)", "Lord(Lance)", "Cleric", "Builder" };
        public static String[] lords = new String[] { "Lord(Sword)", "Lord(Axe)", "Lord(Lance)" };
        public static String[] squires = new String[] { "Cleric", "Builder" };
        public static String[] enemies = new String[] { "Cleric", "Builder" };

        public static String[] names = new String[] { "Erika", "Seth", "Franz", "Gilliam", "Vanessa", "Moulder", "Ross", "Garcia", "Neimi" };

        public int id = -1;
        public Player player = new Player();
        public Location loc = new Location();

        public String name = "Marth";
        public String type = "DummyUnit";
        public Weapon weapon = Weapon.Sword;
        public Mount mount = Mount.None;
        public MoveGroup moveGroup = MoveGroup.Foot;
        public Direction dir = Direction.East;
        public int minRange = 1;
        public int maxRange = 1;
        public int level = 1;
        public int exp = 0;
        public int hp, hpBase, hpCurrent = 1;
        public double hpGrowth = 0.5;
        public int dmg, dmgBase = 1;
        public double dmgGrowth = 0.5;
        public int spd, spdBase = 1;
        public double spdGrowth = 0.5;
        public int def, defBase = 1;
        public double defGrowth = 0.5;
        public int res, resBase = 1;
        public double resGrowth = 0.5;
        public int mov = 1;
        public int sight = 1;
        public int wait = 1;
        public String[] promotions = new String[] { };
        public String special = "Special";
        public String specDesc = "Description";

        public DateTime lastAction = DateTime.MinValue;
        public List<Location> moves = new List<Location>();
        public List<Location> attacks = new List<Location>();
        public List<Location> vision = new List<Location>();

        public Unit() : this(-1) { }
        public Unit(int id) : this(id, new Player()) { }
        public Unit(int id, Player player) : this(id, player, enemies[rand.Next(enemies.Length)]) { }
        public Unit(int id, Player player, String type) : this(id, player, type, new Location()) { }
        public Unit(int id, Player player, String type, Location loc) : this(id, player, type, loc, Direction.East) { }
        public Unit(int id, Player player, String type, Location loc, Direction dir) : this(id, player, type, loc, dir, names[rand.Next(names.Length)]) { }
        public Unit(String type) : this(-1, new Player(), type) { }

        public Unit(int id, Player player, String type, Location loc, Direction dir, String name)
        {
            this.id = id;
            this.player = player;
            this.type = type;
            this.loc = loc;
            this.dir = dir;
            this.name = name;
            level = 1;
            exp = 0;

            switch (type)
            {
                case "Lord(Sword)":
                    createLordSword();
                    break;
                case "Lord(Axe)":
                    createLordAxe();
                    break;
                case "Lord(Lance)":
                    createLordLance();
                    break;
                case "Builder":
                    createBuilder();
                    break;
                case "Cleric":
                    createCleric();
                    break;
            }
        }

        public void levelUp(int times = 1)
        {
            level += times;

            for (int i = 0; i < times; i++)
            {
                hp += rand.NextDouble() < hpGrowth ? 1 : 0;
                dmg += rand.NextDouble() < dmgGrowth ? 1 : 0;
                spd += rand.NextDouble() < spdGrowth ? 1 : 0;
                def += rand.NextDouble() < defGrowth ? 1 : 0;
                res += rand.NextDouble() < resGrowth ? 1 : 0;
            }
        }

        public void promote(String type)
        {
            Unit promote = new Unit(type);
            hp += promote.hpBase - hpBase;
            dmg += promote.dmgBase - dmgBase;
            spd += promote.spdBase - spdBase;
            def += promote.defBase - defBase;
            res += promote.resBase - resBase;

            this.type = type;

            weapon = promote.weapon;
            mount = promote.mount;
            moveGroup = promote.moveGroup;
            minRange = promote.minRange;
            maxRange = promote.maxRange;
            hpBase = promote.hpBase;
            hpGrowth = promote.hpGrowth;
            dmgBase = promote.dmgBase;
            dmgGrowth = promote.dmgGrowth;
            spdBase = promote.spdBase;
            spdGrowth = promote.spdGrowth;
            defBase = promote.defBase;
            defGrowth = promote.defGrowth;
            resBase = promote.resBase;
            resGrowth = promote.resGrowth;
            mov = promote.mov;
            sight = promote.sight;
            wait = promote.wait;
            promotions = promote.promotions;

            hpCurrent = hp;
        }

        public override string ToString()
        {
            return id + " " + player.id + " " + type + " " + (int)dir + " " + name + " " + level + " " + exp + " " + hp + " " + hpCurrent + " " + dmg + " " + spd + " " + def + " " + res;
        }

        public void setStats(int level, int exp, int hp, int hpCurrent, int dmg, int spd, int def, int res)
        {
            this.level = level;
            this.exp = exp;
            this.hp = hp;
            this.hpCurrent = hpCurrent;
            this.dmg = dmg;
            this.spd = spd;
            this.def = def;
            this.res = res;
        }

        private void createStats(int hp, double hpGrowth, int dmg, double dmgGrowth, int spd, double spdGrowth, int def, double defGrowth, int res, double resGrowth)
        {
            this.hp = hpCurrent = hpBase = hp;
            this.hpGrowth = hpGrowth;
            this.dmg = dmgBase = dmg;
            this.dmgGrowth = dmgGrowth;
            this.spd = spdBase = spd;
            this.spdGrowth = spdGrowth;
            this.def = defBase = def;
            this.defGrowth = defGrowth;
            this.res = resBase = res;
            this.resGrowth = resGrowth;
        }

        private void createLordSword()
        {
            weapon = Weapon.Sword;
            mount = Mount.None;
            moveGroup = MoveGroup.Foot;
            minRange = 1;
            maxRange = 1;
            createStats(16, 0.5, 4, 0.5, 9, 0.5, 3, 0.5, 4, 0.5);
            mov = 5;
            sight = 7;
            wait = 5;
            promotions = new String[] { "Greatlord(Sword)" };
            special = "Recruit";
            specDesc = "The Lord can recruit computer controlled units to join their army.";
        }

        private void createLordAxe()
        {
            weapon = Weapon.Axe;
            mount = Mount.None;
            moveGroup = MoveGroup.Foot;
            minRange = 1;
            maxRange = 1;
            createStats(16, 0.5, 4, 0.5, 9, 0.5, 3, 0.5, 4, 0.5);
            mov = 5;
            sight = 7;
            wait = 5;
            promotions = new String[] { "Greatlord(Axe)" };
            special = "Recruit";
            specDesc = "The Lord can recruit computer controlled units to join their army.";
        }

        private void createLordLance()
        {
            weapon = Weapon.Lance;
            mount = Mount.None;
            moveGroup = MoveGroup.Foot;
            minRange = 1;
            maxRange = 1;
            createStats(16, 0.5, 4, 0.5, 9, 0.5, 3, 0.5, 4, 0.5);
            mov = 5;
            sight = 7;
            wait = 5;
            promotions = new String[] { "Greatlord(Lance)" };
            special = "Recruit";
            specDesc = "The Lord can recruit computer controlled units to join their army.";
        }

        private void createBuilder()
        {
            weapon = Weapon.Axe;
            mount = Mount.None;
            moveGroup = MoveGroup.Foot;
            minRange = 1;
            maxRange = 1;
            createStats(16, 0.5, 4, 0.5, 9, 0.5, 3, 0.5, 4, 0.5);
            mov = 5;
            sight = 7;
            wait = 10;
            promotions = new String[] { };
            special = "Build";
            specDesc = "The Builder can create destructable obstacles to block movement.";
        }

        private void createCleric()
        {
            weapon = Weapon.Staff;
            mount = Mount.None;
            moveGroup = MoveGroup.Foot;
            minRange = 1;
            maxRange = 2;
            createStats(16, 0.5, 4, 0.5, 9, 0.5, 3, 0.5, 4, 0.5);
            mov = 5;
            sight = 7;
            wait = 10;
            promotions = new String[] { };
            special = "Heal All";
            specDesc = "The Cleric can heal all allies in a small area around themself.";
        }
    }
}
