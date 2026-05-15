using dc;
using dc.h2d;
using dc.h2d.col;
using dc.ui;
using dc.ui.hud;
using dc.ui.icon;
using HaxeProxy.Runtime;
using ModCore.Utilities;
using Serilog;

using static DeadCellsArchipelago.ImageManager;
using static DeadCellsArchipelago.Translator;

namespace DeadCellsArchipelago {
    public class ItemLine : Line
    {
        public Skill skill;
        public dc.ui.Text text;
        public string itemId;

        public ItemLine(double x, double y, string itemId, int color)
            : base(700, 100, x, y, color)
        {
            bool ctrlShow = false;
            double scaleSkill = 1.0/3;
            skill = new Skill(0, bgBox, new Ref<bool>(ref ctrlShow), new Ref<bool>(ref ctrlShow))
            {
                x = 2,
                y = 1.5,
                scaleX = scaleSkill,
                scaleY = scaleSkill
            };
            this.itemId = itemId;
            double scaleText = 1.0/3;
            text = new dc.ui.Text(null, true, false, new Ref<double>(ref scaleText), null, null);

            if((itemId.Length >= 5 && (itemId[^5..] == "Enter" || itemId[^5..] == " Exit")) || GetBossId().Contains(itemId))
            {
                if(itemId[^5..] == "Enter")
                {
                    int frame = 0;
                    double XY = 0;
                    Tile doorTile = Assets.Class.gameElements.getTile("minimapExit".AsHaxeString(), new Ref<int>(ref frame), new Ref<double>(ref XY), new Ref<double>(ref XY), null);
                    Bitmap doorB = new Bitmap(doorTile, skill)
                    {
                        scaleX = 3,
                        scaleY = 3
                    };
                    CenterX(skill, doorB);
                    CenterY(skill, doorB);

                    string locationName = itemId[..^6];
                    if (IdToNameKeyExist(locationName))
                    {
                        locationName = GetName(locationName);
                    }

                    text.set_text($" {locationName} Enter".AsHaxeString());
                }
                else if (itemId[^5..] == " Exit")
                {
                    int frame = 0;
                    double XY = 0;
                    Tile doorTile = Assets.Class.gameElements.getTile("minimapExitVisited2".AsHaxeString(), new Ref<int>(ref frame), new Ref<double>(ref XY), new Ref<double>(ref XY), null);
                    Bitmap doorB = new Bitmap(doorTile, skill)
                    {
                        scaleX = 3,
                        scaleY = 3
                    };
                    CenterX(skill, doorB);
                    CenterY(skill, doorB);

                    string locationName = itemId[..^5];
                    if (IdToNameKeyExist(locationName))
                    {
                        locationName = GetName(locationName);
                    }

                    text.set_text($" {locationName} Exit".AsHaxeString());
                }
                else
                {
                    Icon bossIcon = Icon.Class.createMobIcon(itemId.AsHaxeString(), skill);
                    double px = 0.5;
                    double py = 0.5;
                    bossIcon.setCenterRatio(new Ref<double>(ref px), new Ref<double>(ref py));
                    skill.setBmpIcon(bossIcon);
                    skill.icon.scaleX = 1.5;
                    skill.icon.scaleY = 1.5;
                    text.set_text($" {Lang.Class.t.texts.get((dc.String)Data.Class.mob.byId.get(itemId.AsHaxeString()).name)}".AsHaxeString());
                }
            }
            else
            {
                skill.setItemIcon(itemId.AsHaxeString());
                text.set_text($" {Lang.Class.t.texts.get(((dc.String) Data.Class.item.byId.get(itemId.AsHaxeString()).name).ToString().Trim().AsHaxeString())}".AsHaxeString());
            }
            skill.btn.visible = false;
        }

        public override void AddParent(dc.h2d.Object parent)
        {
            base.AddParent(parent);
            bgBox.addChildAt(skill, bgBox.layerCount);
            bgBox.addChildAt(text, bgBox.layerCount-1);

            Bounds boundsSkill = skill.getSize(new Bounds());
            text.x = boundsSkill.xMax + skill.x;
            text.y = (boundsSkill.yMax - (text.get_textHeight()*text.scaleX))/2 +skill.y;
        }

        public List<string> GetBossId()
        {
            return ["Behemoth", "Beholder", "TimeKeeper", "Giant", "KingsHand", "Collector", "MamaTick", "GardenerBoss",
            "AmazonSurvival","AmazonTactic", "AmazonBrutal", "Queen", "Death", "DookuBeast"];
        }

        public void Highlight()
        {
            text.set_textColor(16776960);
        }

        public void StopHighlight()
        {
            text.set_textColor(16777215);
        }
    }
}