1. Product Requirements Document (PRD)


# Title: Antz (Ant Colony Strategy Simulation Game (3D))


## 1. Overview

1. A 3D real-time strategy simulation game where the player oversees an ant colony. The player has no direct control over individual ants but influences their behavior through goal-setting, evolution, and resource management.


## 2. Core Gameplay Features

1. 2.1 Autonomous Ant Behavior

2. Ants behave autonomously based on goals, surroundings, and individual stats.

3. Player only influences behavior by assigning high-level goals and modifying evolution traits.

4. Terrain changes occur automatically as ants interact with it.

5. Ant movement and decisions are made using the Ant Colony Optimization (ACO) algorithm

6. The ants use the following pheromones: exploring, found food, food depleted, enemy found, hungry.

7. The pheromones' potency should decrease over time

8. When an ant is moving over an existing pheromone trail it shall strengthen it's potency.

9. Several pheromones can exist in the same location.

10. 2.2 Goal Assignment System

11. Player can assign high-level goals to the colony:

12. Gather food

13. Attack/Defend

14. Explore

15. Expand territory

16. Right-clicking an object or ant (enemy/friendly) brings up context-sensitive actions (e.g., attack, defend, gather).

17. (?) Player can assign roles (scout, gatherer, soldier, builder, caretaker) to each antâ€”designating their purpose

18. Each ant can pickup items- food, eggs, dirt, etc..

19. The amount of items of the same type is tied to it's strength.

20. 2.3 Ant Stats

21. Each ant has individual stats, including:

22. ID

23. Location

24. Health

25. Energy

26. Genome

27. Age (purely representational, not related to longevity)

28. 2.4 Evolution System

29. 2.4.1 Mating Process

30. Player selects a mate for the queen.

31. The selected mate's stats affect the genetic makeup of the next generation.

32. 2.4.2 Egg Generation & Mutation

33. The player determines how much food to invest in egg production.

34. Investment affects how many genome modification points are available.

35. Eggs can be customized with trait mutation sliders.

36. Sliders define the mutation value for each trait.

37. The mutation range is defined by the Mutation Rate genome of the queen

38. Mutation range is expandable or reducible with exponentially scaling food cost.

39. 2.4.3 Egg Hatching

40. Eggs spawn around the queen.

41. The queen gets weakened by #eggs x OffspringTax - (remaining food points / ???). This amount shall first be reduced from the queen's energy/food and then, if greater than the stored energyâ€”from the queen's health.

42. Hatching time is randomly selected within the limits set by their genome traits.

43. 2.5 Ant Genome

44. Each ant has a genome that contains its basic traits, including:

45. Size â€“ Affects the actual size of the ant. Allows it to store more energy.

46. Speed â€“ Affects the movement speed and attack speed of the ant. Makes it consume more energy with each movement and attack.

47. Strength â€“ Allows the ant to break harder things and faster. Makes it consume more energy with each action.

48. Sight Range - Allows the ant to notice pheromones further away. Requires more hatch time and makes the ant consume more energy every second.

49. Recovery Kickstart - Determines when the ant starts to heal (health percentage: 0-1)

50. Recovery Speed - Determines how much energy is converted to health

51. Number Of Offspring â€“ The average number of offspring.

52. Offspring Birth Tax â€“ The damage the queen takes upon laying each egg.

53. Offspring Hatch Time â€“ The amount of time it takes the egg to hatch.

54. Mutation Rate â€“ Genomic deviation percentage: how much it can deviate from its parents (0-1)

55. 2.5.1. Hereditary Strength

56. Each genome trait has a hereditary strength. The hereditary strength determines the chance of this genome being passed on. When an egg is created, the game randomizes a number between 0 and the hereditary strength score for the trait from each parentâ€”the parent with the higher random number passes on their genomic trait.

57. Example:

58. Ant#1 has a size trait of 5 with a hereditary strength of 0.8.

59. Ant#2 has a size trait of 2 with a hereditary strength of 0.5.

60. Ant#1 randomizes a number: 0.45

61. Ant#2 randomizes a number: 0.31

62. The new offspring shall have a base size trait of 5 with a hereditary strength of 0.8.

63. 2.5.2 Mutations

64. After the base genome has been built from its parents' genomes, mutations occur:

65. Each trait is randomized around its base value.

66. The mutation range is determined by the Mutation Rate genome and the player's input in the genome editor.

67. 2.6 Energy 

68. With each tick, each ant's energy is reduced relative to it's traits (size, speed, etc.).

69. If an ant's energy is at 0- it starts to lose health rapidly.

70. When an ant's energy is higher than <Recovery Kickstart gene>% it starts to heal it's wounds.

71. while healing- The energy consumption is multiplied by <Recovery Rate gene> and this extra goes into the ant's health points until fully healed.


## 3. GUI Design

1. This section details the progressive refinement of gameplay systems, visuals, and user interfaces.

2. 3.1 Game Image

3. 3D Render / Concept Art Placeholder Image:

4. This image represents the evolving visual fidelity and immersive environment of the game. Terrain layers, ant units, fog of war, and colony structures are visualized in a stylized, realistic art direction.

5. 3.2 GUI (Graphical User Interface)

6. The user interface is a key component of the gameâ€™s usability and player immersion. The GUI consists of layered, context-sensitive panels and overlays that enable interaction without disrupting the simulation.

7. 3.2.1 Main Menu

8. The game shall present a main menu when launched.

9. Options include:

10. New Game â€“ Starts a new colony.

11. Load Game â€“ Opens the save file browser.

12. Settings â€“ Allows configuration of audio, graphics, and input.

13. Exit Game â€“ Closes the application.

14. 3.2.2 Main HUD Elements

15. Mini-map: Shows explored areas, ant positions, and colony highlights.

16. Resource Panel: Displays food reserves, current population, egg counts.

17. Goal Toolbar: Quick-access bar for assigning goals like Explore, Gather, Attack, etc.

18. Ant Role Panel: Displays role distribution (gatherers, soldiers, etc.) and allows reassignment.

19. 3.2.3 Context Menus

20. Right-clicking on terrain, enemies, or structures opens radial or vertical context menus.

21. Actions include "Attack Here," "Defend This Area," "Harvest Resources," or "Expand Tunnels."

22. 3.2.4 Overlays

23. Goal Assignment Overlay: High-level strategy assignment window.

24. Genome Editor Overlay: Trait sliders appear when editing egg traits.

25. Mate Selection Panel: Displays available mates and their genome traits.

26. Egg Batch Panel: Shows all currently developing eggs and sliders for investment.

27. 3.2.5 Fog of War

28. Only explored regions are visible to the player.

29. Regions without any ants are covered in fog.

30. 3.2.6 Feedback and Visualization

31. Floating UI elements over ants indicate roles, status (tired, injured), or tasks.

32. Hovering over elements brings up tooltips and genome-related data.

33. Visual alerts for major events (queen injured, eggs hatching, enemies detected).

34. 3.3 User Controls

35. Scroll Wheel: Move up/down terrain layers.

36. Left Mouse + Drag/QE: Rotate camera.

37. Right Mouse/WASD/Arrow Keys: Move camera in-plane.

38. Scroll Wheel or +/-: Zoom in/out.


## 4. Dependencies

1. Unity 3D Engine

2. Pathfinding package for tile-based terrain

3. Serialization system for game saves


## 5. Assumptions and Constraints

1. No direct control of ants will ever be implemented.

2. Initial release targets single-player only.

3. Simulation accuracy takes precedence over visual fidelity.


