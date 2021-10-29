# Genshmup

A Genshin Themed Danmaku Shooter written in C# .NET 6 using GDI+
Officially 0 Warnings AND 0 Messages in Visual Studio haha take that

![Ganmyu](./Genshmup/Assets/ganyu.png)

## Controls
### Stage

**Arrow Keys** - Move Player  
**Y, Z, Enter** - Shoot  
**E** - Use Elemental Skill  
**Q** - Use Ultimate Skill  
**Shift** - Move slower and show Hitbox  
**Escape** - Pause  
**S** - Skip Dialog

### Menus

**Arrow Keys** - Select  
**Enter** - Confirm

## Mechanics Help

#### The Stage Screen

**Health Stats**  
At the top there is a bar indicating the Boss' health and next to it is your life count  

**Skills**  
In the bottom right there are 2 circles indicating how charged your skills are

#### How to fight

You shoot regular bullets using Y, Z or Enter  
If you hit the boss the boss loses health and you gain elemental energy  
Elemental energy allows your "Q", or Ultimate Skill, to charge up further  
with your "E", or Elemental Skill, you can gain a lot of that Elemental energy with a hit  

Your "E" provides a shield that can protect you against a single hit

Your "Q" provides a shield that lasts a certain time and can deal infinite hits  
It also clears every bullet in a small radius

When you defeat a boss you'll automatically be loaded into the next stage
