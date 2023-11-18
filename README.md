# Rogue Adventure (WIP)
Based of the mobile game 'Rogue Adventure'

# TODO
## Very important
- [ ] Fix SFX
- [ ] Change the reward system to include a risk level
- [ ] Change enemy intent system to a method (give context to the card)
- [ ] Fix animation glitch
- [ ] Fix when the effects get depete
- [ ] Add final boss
- [ ] Add starting node (give cards)
- [ ] Make enemies fair
- [ ] Fix Weakness
- [ ] Add menu before starting a game (to set seed)
- [ ] Clean classes (moving tasks to new managers)
- [ ] Add screen vibrations on death
- [ ] Add sounds
- [ ] Make the random offset system more complexe
## Fairly important
- [ ] Rogue Class effects
- [ ] Skills
- [ ] Resistance effect
- [ ] Curse effect
- [ ] Disable all keybinds (or redirect some)
- [ ] Merchant screen (copy the btd6 shop)
- [ ] Change the enemy group api
- [ ] Add particle for effects
## Slightly important
- [ ] Class selection menu
- [ ] Save file system
- [ ] Change the rewards to look like lootboxes
- [ ] Replace curtain bottom
- [ ] Replace curtain top
- [ ] Replace reward ui title bg
- [ ] Replace reward item banner
## Not important
- [ ] Invistigate why Blur mat gets loaded twice
# Completed
- [x] Remove default UI
- [x] Display UI
- [x] Disable tower keybinds (side effect from removing default UI)
- [x] Display enemy cards
- [x] Display hero cards
- [x] Mana management
  - [x] Remove mana when card played
  - [x] Prevent play if no more mana  
- [x] Damage enemy
- [x] Replace reward bg
- [x] Enemy generator (Picks a random group and spawns it)
- [x] Card management
  - [x] Remove played card
  - [x] Refill hand
  - [x] Empty discard if necessary
- [x] Enemy Death Animation
- [x] No infinite battles
- [x] Enemy Shield
- [x] Change the camera pov to cover the black void
- [x] Background blur (make it darker)
- [x] Change the effect system to an event based
- [x] Choose card
- [x] Choose enemy target
### Additions
- [x] Add the Slash texture on nodes won
- [x] Add pop up for cards' description
- [x] Add color changing number depending on increase/decrease
- [x] Add posibility to restart by quiting
- [x] Add death
- [x] Add random offset to the map nodes
- [x] Add World class
- [x] Add more than 5 lanes
### Removes
- [x] Remove blur if game is load not from me
- [x] Remove track arrows
- [x] Remove CHIMPS popup
### Generators
- [x] Generate rewards
- [x] Level generator
### Ports
- [x] Port HeroCard to ModContent
- [x] Port EnemyAction to ModContent
- [x] Port RogueClass to ModContent
- [x] Port EnemyCard to ModContent
### Fixes
- [x] Fix swing animation
- [x] Fix shied ui on death
- [x] Fix effect level text
- [x] Fix Map movement
- [x] Fix rewards UI (make it cover the entire screen)
- [x] Fix thorns (dont get clear on round end)
- [x] Fix enemy HP bar
- [x] Fix color damage alteration
- [x] Fix Prefab multiplication
### End turn button
- [x] Replace end turn btn display
- [x] Disable end turn btn when enemy turn
### Effects
- [x] Weakness effect
- [x] x2 Damage effect
- [x] Strength effect
- [x] Armored effect
- [x] Wound effect
- [x] Burn effect
- [x] Poison effect
- [x] Frail effect
- [x] Thorns effect
- [x] Card descriptions
- [x] Immune effect
- [x] No Primary Effect
- [x] No Military Effect
- [x] No Magic Effect
- [x] No Support Effect
- [x] No Items Effect
- [x] No Hero Effect
- [x] No Paragon Effect
- [x] Block Primary Effect
- [x] Block Military Effect
- [x] Block Magic Effect
- [x] Block Support Effect
- [x] Block Items Effect
- [x] Block Hero Effect
- [x] Block Paragon Effect
- [x] Overcharged effect

# Canceled
Nothing
