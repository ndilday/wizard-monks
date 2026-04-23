# Order of Wizard Monks

## Simulation Engine — Product Requirements Document

**Project:** Order of Wizard Monks - Simulation Engine  
**Version:** 1.5  
**Date:** 2026-03-25  
**Author:** NAD  
**Status:** In Development  

---

## Table of Contents

1. Introduction & Vision
   - 1.1. Vision
   - 1.2. Problem Statement
   - 1.3. Target Persona
2. Core Systems & Features
   - 2.1. Character Lifecycle Simulation
   - 2.2. AI Decision Engine
   - 2.3. Gameplay Mechanics & Activities
   - 2.4. Covenants
   - 2.5. Economic Simulation
   - 2.6. Knowledge, Belief & Reputation
   - 2.7. Magical Tradition System
   - 2.8. Legal System
   - 2.9. World Simulation
   - 2.10. Geography & Tribunals
3. Current Scope & Limitations
4. Future Considerations

---

## 1. Introduction & Vision

### 1.1. Vision

To create a comprehensive, data-driven simulation engine that models the lives, careers, and legacies of magi within the world of the Ars Magica Role Playing Game. While the initial scenario is grounded in the Ars Magica setting, the underlying architecture is designed to be setting-agnostic: the core abstraction is a world of distinct magical traditions coming into contact, exchanging knowledge, and evolving over time. The engine will serve as a tool for generating emergent narratives, exploring long-term character development, and simulating the evolution of a covenant or even the entire Order of Hermes over decades or centuries.

### 1.2. Problem Statement

Tracking the complex, long-term progression of multiple magi, their research, their resources, and their relationships over a multi-generational saga is a significant bookkeeping challenge. The simulation engine automates this process, allowing a user (the "Storyguide" or "Simulation Steward") to observe the organic development of a world based on established rules and character motivations.

### 1.3. Target Persona

The Simulation Steward: A user familiar with the Ars Magica setting who wants to generate a "living world." They will initialize the simulation with starting characters and parameters, advance the world season by season, and use the generated logs and character states to inform a role-playing game, write stories, or simply observe the outcomes.

---

## 2. Core Systems & Features

The product is a headless simulation engine. Its features are the systems that govern the behavior of the simulated world and its inhabitants.

---

## 2.1. Character Lifecycle Simulation

The engine simulates the complete lifecycle of a character from creation to death.

### 2.1.1. Character Model

Characters are defined by a detailed set of properties:

- **Attributes:** The eight core Ars Magica attributes, modeled with a base value and a decrepitude system.

- **Abilities & Arts:** A comprehensive list of skills and magical arts. Advancement is experience-based and supports both standard and accelerated progression rates.

- **Personality:** Characters are modeled using the HEXACO personality framework — a six-factor, 24-facet model. HEXACO was chosen over the Big Five because it has demonstrated stronger predictive validity for real-world behavioral outcomes in psychological research, which aligns with the simulation's core goal of deriving believable behavior from personality. The six factors are Honesty-Humility, Emotionality, Extraversion, Agreeableness, Conscientiousness, and Openness to Experience, each comprising four specific facets. Facet scores are continuous values ranging from approximately 0 to 2, with a mean of 1.0. A score of 1.0 represents an average disposition and has no effect on behavior; scores above or below this influence the character's behavior through the mechanisms described below.

  Personality influences AI decision-making at multiple points in the cognitive architecture (see §2.2). Rather than acting as direct desire multipliers, facets shape behavior through several distinct mechanisms: they govern the thresholds at which emotion tokens trigger goal generation — for example, high Inquisitiveness lowers the threshold for generating research goals from mild curiosity; they influence commitment strength at intention formation — high Prudence produces more durable commitments to self-preservation goals; they shape self-belief formation during reflection — a character with high Modesty is less likely to form strong beliefs about their own prestige; and they determine the form goals take once generated — high Sociability directs relationship goals toward cooperation rather than competition. Personality does not modulate emotion intensity directly.

  Procedurally generated characters receive randomized facet scores drawn from a normal distribution. Named characters such as the founders have hand-authored personality profiles. Personality is not currently modeled as changing over a character's lifetime, though this is a natural future extension.

  **Personality Phase 2.** The design intent is for personality to evolve over a character's lifetime in response to significant experiences. Formative events — completing a major breakthrough, losing an apprentice, suffering repeated aging crises, or achieving a long-sought goal — should be capable of shifting facet scores over time. The direction and magnitude of change should itself be personality-dependent: a character with high Openness is more likely to be changed by new experiences than one with low Openness. The design challenge is defining what constitutes a significant event, how large a shift is plausible, and how to avoid personality drift that undermines character identity. This requires a dedicated design phase before implementation.

  Interpersonal rivalries are a natural long-term goal for the personality system. Two magi who repeatedly compete for the same resources, hold conflicting beliefs about each other, or have a history of legal conflict should develop persistent antagonisms that color their future interactions and decision-making. These rivalries are not yet modeled but represent a significant design direction once the belief system, personality evolution, and legal systems are sufficiently developed. See also §2.8 for the role of Wizard War as a formal expression of such antagonisms.

- **Health & Aging:** An automated aging system triggers annually after age 35. Each year, a stress die is rolled, modified by the character's age and the strength of their Longevity Ritual (if any). Results below 3 grant a season of no apparent aging. Results above 9 add a decrepitude point. Results of 13 or above 21 trigger a crisis: the Longevity Ritual is voided, decrepitude jumps to the next threshold level, and a survival roll against Stamina determines whether the character dies. A character with a Longevity Ritual gains Warping experience each aging cycle. Death occurs when decrepitude reaches 75. Decrepitude thresholds are currently tracked as a single score with five levels (5, 15, 30, 50, 75); distribution of decrepitude points to individual attributes is a planned future enhancement.

- **Magic:** Magi possess a repertoire of known spells, vis stockpiles, and a MagicalTradition object (see §2.7) that encapsulates the principles and capabilities of their practice.

### 2.1.2. World Population

The engine supports the procedural generation of non-player characters to populate the world.

**Founders.** The twelve Founders of the Order of Hermes are hand-authored characters with individually crafted personality profiles, attribute scores, and starting ability distributions. They are initialized at staggered ages and states reflecting the historical arc of the Founding — the earliest recruits are older and further developed when the scenario begins, matching the roughly thirty-year period over which Trianoma gathered them before the formal founding in 767 AD.

**Hedge Magi.** The simulation supports a hedge mage character type distinct from a full Hermetic magus. A hedge mage belongs to a HedgeTradition — a named magical tradition carrying a description, a set of conversion bonuses applied when the mage joins the Order, and a list of potential breakthroughs that any interested magus can pursue by studying that tradition. The HedgeTradition model is the bridge between the recruitment system and the Magical Tradition research pipeline (see §2.7).

Procedural hedge mage generation is not yet implemented. Each founder's pre-Hermetic magical tradition needs to be defined as a HedgeTradition instance before the Founding scenario can run fully.

### 2.1.3. Seasonal Progression

The simulation advances in discrete turns. The current clock tick is one season; in each tick, a character performs one major activity. The tick duration is an abstraction — see §2.2.1 for the design intent around time-scale independence.

### 2.1.4. Warping & Twilight

Warping represents the cumulative toll of sustained exposure to magical forces. Every character has a Warping Score, increased by Warping Points in the same way an Ability increases by experience points — each new point of Warping Score costs five times the new score in Warping Points (e.g., reaching Score 3 from Score 2 requires 15 points). Warping Points are gained from several distinct sources, and all sources stack.

**Phase 1 Sources.** The following Warping Point sources are relevant to the current simulation and will be tracked in Phase 1:

**Living in a strong aura.** A mage who lives and works within a magical aura of strength 6 or higher gains Warping Points each year based on the aura strength and the fraction of time spent within it. Hermetic magi do not gain Warping Points from living in a magical aura — their Gift makes them native to it. This distinction matters when covenfolk are eventually modeled.

**Longevity rituals.** A mage under the effect of a longevity ritual gains one Warping Point per year as a continuous magical effect. This is already partially tracked in the aging system and will be formalized under the Warping system in Phase 1.

**Original research botches.** When a mage botches during original research, they gain one Warping Point per zero on the botch dice. If two or more Warping Points are gained from a single botch event, a Twilight check is triggered. Original research is the first activity in the simulation where a Warping-related botch can occur, making it the natural entry point for the Twilight system.

**Phase 2 Sources (deferred).** Being the target of a powerful spell not designed for the caster, and botching a spell cast, are also canonical sources of Warping Points. These require the spell-casting system to be more fully developed and are deferred to a future phase.

**Wizard's Twilight.** Whenever a Hermetic magus gains two or more Warping Points from a single event, they must roll to avoid Twilight. Avoidance roll: Stamina + Concentration + Vim score + stress die vs. Warping Score + Warping Points gained + local aura + stress die (no botch). If the roll succeeds, the mage spends a moment regaining control with no further effect. If it fails, the mage enters Twilight. A botched avoidance roll means the mage enters Twilight and cannot comprehend the experience.

While in Twilight, a mage is completely unavailable for seasonal activity, trading, experience gain, or any other simulation action. The duration of Twilight depends on the mage's Warping Score and the outcome of a comprehension roll (Intelligence + Enigmatic Wisdom + stress die vs. Warping Score + stress die). Duration ranges from minutes at low Warping Scores to seasons, years, or decades at higher scores. A botched comprehension roll extends the duration upward on the table. At Warping Score 10 or above with a failed comprehension roll, the mage enters Final Twilight — permanent disappearance and effective death to the simulation.

A comprehended Twilight leaves a beneficial Twilight Scar and may grant bonus experience or a new spell. A failed comprehension leaves a detrimental Scar and may cause loss of experience or spells. These effects are rolled for magnitude on a simple die.

**Enigmatic Wisdom.** Criamon magi have the Enigmatic Wisdom ability, which both aids Twilight comprehension and makes Twilight more likely. This will be factored in when the Mysteries system is implemented.

**Future Phases.** Warping effects on mundane characters (Flaws gained at Warping Score 1, 3, 5, 6+) are deferred until covenfolk are modeled. Lab Warping (the Warping stat on laboratories, see §2.3.2) is tracked but not yet connected to this system.

---

## 2.2. AI Decision Engine

The core of the simulation is a layered cognitive architecture that determines each character's goals and activities each clock tick. The architecture is organized into three layers that operate in sequence: a **Memory & Belief** substrate that accumulates and synthesizes experience; an **Appraisal & Emotion** engine that evaluates events against the character's goals and standards; and an **Intention & Goal Generation** layer that produces and commits to goals, feeding the execution hierarchy below it.

The existing execution hierarchy — Goal → Condition → Helper → Activity — is preserved as the bottom layer of this stack. What changes is everything above it: the machinery that determines which goals a character forms, and why.

---

### 2.2.1. Memory & Belief

Every character maintains a **memory stream**: a timestamped, append-only log of significant events, experiences, and observations. Each entry carries an **importance weight** — a scalar representing how significant this event was to this character at the time it occurred. Importance is not a global property of an event; the same event may be highly important to one character and trivial to another, depending on their current goals, emotional state, and personality.

Memory entries are not raw facts. Each entry is an **interpreted record**: it captures not only what happened, but the character's appraisal of it at the time — which goals it was relevant to, what emotion it produced, and whether it confirmed or challenged an existing belief. This interpretation is performed by the Appraisal Engine (§2.2.2) at the moment the event is recorded.

**Belief Formation.** Beliefs are not asserted directly into a character's belief profile. They are *derived* from the memory stream through a periodic **reflection** process. During reflection, the character synthesizes patterns across recent memory entries, updating or revising beliefs based on accumulated evidence. Beliefs formed through reflection carry a confidence value that reflects the weight of evidence behind them. A belief held with low confidence is more susceptible to revision than one reinforced by many consistent memories.

The current purely-additive belief update model — where reading a book propagates the author's beliefs directly into the reader's profile — is replaced by this reflection-based model. Book-derived beliefs and lab-text-derived beliefs now enter the memory stream as high-importance entries rather than updating belief profiles directly, and are synthesized into beliefs during the next reflection pass.

**Reflection Cadence.** Two approaches are viable and should be evaluated during implementation:

- *Threshold-triggered reflection:* Reflection fires when the total accumulated importance of unprocessed memory entries crosses a threshold. This mirrors the Generative Agents model and produces reflection as a natural response to a dense cluster of significant experiences. It is more realistic but more complex to tune, and its behavior changes character if tick duration changes.
- *Tick-synchronized reflection:* Reflection fires once per tick. At seasonal granularity this is narratively coherent — a mage working alone in a laboratory for an entire season has ample time for introspection. It is simpler to reason about and test, but may not remain appropriate if tick duration is reduced.

The PRD does not prescribe which approach to implement first. The reflection interface should be designed so that the cadence trigger is a replaceable policy, not hardcoded into the reflection logic.

**Self-Beliefs.** Characters maintain belief profiles not only for other entities they have encountered, but for themselves. The self-belief profile uses identical data structures to beliefs held about others, with the character treating themselves as a belief target. Self-beliefs capture assessments that are not directly representable in the character's mechanical stat block: how skilled the character believes themselves to be relative to their peers, which domains they consider their strengths and weaknesses, how they interpret their own reputation, and how they understand their trajectory over time.

The self-belief profile is the primary input to the appraisal engine for goal generation. The character's actual stat block remains the ground truth for mechanical resolution. This creates a deliberate **belief-reality gap**: a mage who has internalized a belief that they are a strong Creo specialist will generate Creo-oriented goals based on that self-belief, even if their actual Creo score is unremarkable. Events that produce outcomes inconsistent with the self-model — a Creo project that fails repeatedly — generate high-importance memory entries that drive reflection toward revising the self-belief. The gap narrows or widens organically through experience.

**Memory Management.** The memory stream is bounded by importance-weighted forgetting. Low-importance entries are candidates for expiration after a configured number of ticks. During reflection, processed entries whose importance has not been refreshed by subsequent corroborating events are consolidated: the beliefs and emotion tokens they contributed are retained, but the raw entry is discarded. High-importance entries — those that generated strong emotion tokens, triggered belief revision, or were corroborated by multiple subsequent events — are retained indefinitely. The effective memory of a long-lived character therefore consists of a compressed record: vivid formative experiences, repeatedly-confirmed beliefs, and the emotional residue of significant events, with the routine and the trivial having faded.

---

### 2.2.2. Appraisal & Emotion

Every event that enters a character's memory stream is evaluated by the **Appraisal Engine** before it is recorded. Appraisal follows the Ortony, Clore & Collins (OCC) cognitive model: an event is assessed along three axes — its relevance to the character's current goals, its attribution (who or what caused it), and whether it conforms to the character's personal standards as expressed through their HEXACO personality profile. The result of this assessment is one or more **emotion tokens**, each with a type, an intensity, and a decay rate.

**Emotion Types.** The OCC model produces twenty-two distinct emotion types. The subset relevant to this simulation is:

- *Hope / Fear* — anticipation of a desirable or undesirable future event, proportional to probability and stakes.
- *Joy / Distress* — response to a confirmed positive or negative outcome relevant to a current goal.
- *Pride / Shame* — self-attribution of a positive or negative outcome; the character credits or blames themselves.
- *Admiration / Reproach* — attribution of a positive or negative outcome to another character.
- *Gratitude / Anger* — response to a beneficial or harmful action by another character directed at oneself.
- *Envy / Gloating* — response to a positive or negative outcome for another character in a domain the observer cares about.

**Emotion Intensity.** The intensity of an emotion token at the moment of appraisal is determined by the event's relevance to the character's currently active goals and beliefs — how much is at stake, how central the affected goal is, and how directly the event bears on the character's self-model. Personality does not modulate intensity directly; its influence on emotional life is expressed elsewhere in the stack, through goal generation thresholds, commitment strength, self-belief formation, and what kinds of events a character registers as goal-relevant in the first place.

**Emotion Token Lifecycle.** Emotion tokens are continuous rather than discrete. Each token carries a current intensity (a float) and a decay rate (a float). At the start of each tick, before goal recalculation, all active emotion tokens are decayed by their individual rates. Tokens whose intensity falls below a minimum threshold are expired and removed. New events may add to the intensity of an existing token of the same type rather than creating a duplicate. Decay rates are properties of the emotion type and event context, not personality variables.

**The Emotion-Belief Relationship.** Emotion tokens and beliefs are not independent. An event that generates a significant emotion token also generates a high-importance memory entry, ensuring that emotionally significant experiences accumulate toward belief revision during reflection. The intensity of the emotion token influences the importance weight of the corresponding memory entry: a high-intensity Anger event produces a higher-importance memory than a low-intensity one. This coupling ensures that long-term belief evolution tracks emotional experience over time, rather than treating affect and cognition as separate channels.

**Goal Modulation.** Active emotion tokens modulate both goal generation and desire scores in the Intention & Goal Generation layer (§2.2.3). The specific mappings from emotion types to goal generation tendencies are described there, under each goal category.

---

### 2.2.3. Intention & Goal Generation

The **GoalGenerator** is responsible for producing the goals that feed the execution hierarchy. In the revised architecture, it is driven by the outputs of the Appraisal Engine and the character's belief profile, rather than by direct state checks against the character's mechanical stats.

**From Appraisals to Intentions.** At the start of each tick, after emotion decay and before activity selection, the GoalGenerator evaluates the character's current emotion tokens, self-belief profile, beliefs about others, and personality profile to determine which new goals, if any, should be formed. This evaluation is not a checklist. It is a weighted generative process: emotion tokens and beliefs contribute motivational pressure toward certain goal types, and a goal is generated when that pressure crosses a threshold appropriate to the character's personality. A character with high Inquisitiveness has a lower threshold for generating research goals from mild curiosity; the same level of curiosity would not move a low-Inquisitiveness character to act.

**Intention as Commitment.** A goal that has been generated and accepted by the character becomes an **intention** — a committed plan of action that is not re-evaluated from scratch each tick. This is the key departure from the previous architecture, where desire scores were recomputed every tick from raw state and any goal could be displaced at any moment. Intentions are sticky: once formed, a goal persists unless a **reconsideration trigger** fires. Reconsideration triggers include:

- A new event whose importance weight exceeds the current intention's commitment strength.
- A belief revision — produced by reflection — that undermines the reasoning behind the intention.
- An emotion token of sufficient intensity that conflicts with the intention (e.g., Fear of a consequence that the intention would produce).
- The passage of a configured maximum duration without meaningful progress, modulated by the character's Prudence facet.

**Commitment Strength.** Each intention carries a commitment strength scalar that governs how easily it can be displaced by reconsideration triggers. Three candidate models for how commitment strength is set and evolves are deferred to implementation for evaluation:

- *Formation-fixed:* Commitment strength is set once at intention formation from the intensity of the generating emotion token and personality modifiers, then held constant for the life of the intention.
- *Decaying:* Commitment strength begins at the formation value and decays over time, making older intentions progressively easier to displace — unless reinforced by new corroborating emotion tokens or memory entries.
- *Investment-accruing:* Commitment strength begins low and increases as the character invests ticks into pursuing the intention, modeling the psychology of sunk cost. Sunk investment makes abandonment feel increasingly costly, up to a ceiling modulated by Prudence.

These models are not mutually exclusive. A hybrid — formation-fixed base, modified by accrued investment — may prove most behaviorally realistic.

**Goal Types and Their Generative Conditions.** The following describes the primary generative conditions for each broad goal category.

- *Self-preservation goals* (longevity, health): Generated primarily from Fear tokens triggered by aging events, aging crises observed in others, or self-beliefs about vulnerability. Personality shapes how readily a character generates these goals — through the goal generation threshold and commitment strength — rather than by directly modulating the Fear tokens themselves.

- *Research and mastery goals*: Generated from the interaction of Inquisitiveness, Pride in past achievements, and self-beliefs about one's own areas of strength and aspiration. A character who believes they are a strong specialist in a domain and experiences Pride from a success in that domain is more likely to generate a further research goal within it. Envy of another's achievement in a domain the character cares about can also generate competitive research goals — the character is motivated not only by curiosity but by the desire to not be surpassed.

- *Social and relationship goals*: Generated from the full range of interpersonal emotion tokens, mediated by Sociability, Fairness, Patience, and Forgivingness. The generative conditions are richer than cooperative goals alone:

  - *Gratitude* toward another character — amplified by Fairness — generates cooperative or reciprocal goals: seeking to assist, teach, or benefit the target.
  - *Admiration* of another character — amplified by Sociability — generates goals oriented around proximity and learning: seeking that character's company, studying under them, or drawing on their expertise.
  - *Envy* of another character's achievements or standing — amplified by low Fairness and high Social Self-Esteem — generates antagonistic or competitive goals: seeking to surpass the target, undermine their reputation, or acquire what they possess. The specific form of the goal is shaped by Patience and Forgivingness: a character low in both may generate overtly hostile goals; a character higher in these facets may sublimate envy into private competition without direct antagonism.
  - *Fear* of a specific character generates avoidance or appeasement goals rather than cooperative ones. A character who fears another may seek to reduce contact, offer concessions, or build alliances against the feared party. The same personality facets that otherwise drive cooperative behavior may instead push fearful characters toward alliance-building as a protective strategy.
  - *Anger* toward another character — amplified by low Patience and low Forgivingness — generates hostile or retaliatory goals. Unlike Envy, which is comparative and status-driven, Anger is reactive and harm-directed: the character wants to respond to a perceived wrong rather than simply to come out ahead.
  - *Fairness* as a standalone driver generates altruistic goals around other characters' needs, independent of any specific emotion token. Characters with high Fairness observe another's need and generate helping goals without requiring a personal emotional trigger. This remains the primary mechanism for modeling cooperation that is not rooted in self-interest.

- *Recognition and reputation goals*: Generated from the interaction of Pride, the character's prestige motivation (derived from low Modesty and high Social Self-Esteem), and self-beliefs about how their reputation compares to their aspirations. A character who believes their reputation understates their actual accomplishments is more likely to generate publication or demonstration goals.

- *Reactive and crisis goals*: Generated by high-intensity emotion tokens of any type in response to external events injected by the Scenario Manager (§2.9.1). These goals preempt current intentions only if their motivational weight exceeds the commitment strength of the displaced intention.

**Relationship Goals.** Goals oriented around another character's state are now a natural output of the appraisal-driven generative process rather than a special subclass. A character generates a goal around another's situation when their belief about that character's circumstances produces a sufficiently strong emotional response, shaped by their personality profile. The mechanism is identical to self-oriented goal generation; the distinction is only in the target.

---

### 2.2.4. Execution Hierarchy

The goal execution layer is preserved from the previous architecture with one structural clarification: it operates on **intentions**, not raw goals. An intention carries not only the goal's target state and desire score, but its commitment strength and the conditions under which it may be reconsidered. The execution layer has no visibility into the appraisal or generation machinery above it; it receives a prioritized list of active intentions and selects activities accordingly.

- **Conditions:** Prerequisites and sub-evaluations that must be satisfied before an activity can be selected for a given intention (e.g., `HasLabCondition`, `VisCondition`).
- **Helpers:** Logic modules that evaluate and score candidate activities against the active intention.
- **Activities:** Discrete, single-tick actions (e.g., `ReadActivity`, `InventSpellActivity`). Activities remain atomic and uninterruptible within a tick. The decision about whether to continue a multi-tick project is re-evaluated at the next tick's intention reconsideration pass.

**Desire Scores.** Each intention retains a desire score, recomputed at the start of each tick from the character's current emotion tokens, active beliefs, and personality facets. The desire score governs priority among competing active intentions. It is distinct from commitment strength: a mage may have low current desire for an intention they are still committed to — they are tired of the work — without that intention being reconsidered, because they have sunk significant investment and their Prudence is high.

**Clock Tick.** The simulation currently operates at seasonal granularity — each tick represents one season. The architecture is designed so that the tick duration is an abstraction: the decision-evaluation loop, intention reconsideration, and activity selection all reference "the current tick" rather than "the current season," allowing finer-grained time scales to be introduced in future phases without restructuring the decision engine.

---

### 2.2.5. Collaborative Logic

The AI supports mandatory, two-person activities. An intention held by one character (e.g., a master) can set the mandatory action for both themselves and a second character (e.g., an apprentice), ensuring they perform the same activity in the same tick. The collaborative constraint is evaluated during the execution phase and is not visible to the appraisal or goal generation layers.

---

## 2.3. Gameplay Mechanics & Activities

The engine implements a wide array of activities from the Ars Magica rulebook.

### 2.3.1. Magical Research & Development

**Spell Invention.** Magi invent new formulaic spells through a project-based system. A magus can only attempt to invent a spell if their Lab Total for the relevant Technique and Form combination exceeds the spell's level. Each season of work accumulates progress points equal to the Lab Total minus the spell's level. When accumulated points meet or exceed the spell's level, the spell is learned.

The Lab Total is: Technique + Form + Intelligence + Magic Theory + Aura Strength + laboratory bonuses + lab assistant contribution (Intelligence + Magic Theory). A bonus applies for knowing a similar spell of the same base effect — +1 per 5 levels of the highest known similar spell. Magical foci, non-apprentice lab assistants, and familiars are not yet factored into the Lab Total and are known future additions.

The AI selects a target spell level equal to the highest level completable in a single season — the point at which the Lab Total is exactly double the spell level. The AI will not currently initiate a multi-season invention project. If a usable lab text is available for the desired effect, learning from the text is always preferred over invention from scratch.

**Longevity Rituals.** Magi can invent and apply Longevity Rituals to mitigate aging. A ritual's strength equals the mage's Creo Vim Lab Total divided by 5. The ritual is voided by any aging crisis and must be reinvented.

The AI manages longevity through a permanent, recurring goal that never completes. It dynamically calculates a deadline based on the mage's current age and ritual strength and works backward from that deadline to determine whether to improve the Creo Vim Lab Total now or to perform the ritual immediately. Prudence shapes how early and how durably a character commits to this goal. Vis cost is age-dependent and must be satisfied before the ritual can be performed.

In the future, non-magi characters — such as Redcaps or highly valued covenfolk — may seek out a prominent Creo mage to create a Longevity Ritual on their behalf.

**Tradition Integration.** The core principles of magic are encapsulated in a MagicalTradition object (see §2.7). A tradition can be expanded through research projects that integrate concepts sourced from contact with other traditions, unlocking new capabilities (e.g., new spell ranges, accelerated arts, new lab activities).

**Original Research.** Magi can undertake original research to expand the boundaries of Hermetic magic. Research targets are defined as breakthrough definitions, each specifying a name, a required number of breakthrough points, a set of researchable principles (new spell attributes, new spell bases, or new lab activities), and the Art pairs associated with the work.

Research proceeds through a series of phases, each consuming multiple seasons. In each phase, the research service generates an experimental spell for the researcher to invent and then stabilize. The target spell level is scaled to the researcher's Lab Total, with personality modifiers applied — Creativity pushes toward more ambitious experimental spells, Prudence toward safer ones. Upon stabilization, breakthrough points equal to the spell's magnitude are gained. When accumulated breakthrough points meet or exceed the definition's requirement, the breakthrough is achieved.

On breakthrough, the discovered principle moves from potential to integrated in the researcher's magical tradition, activating its mechanical effect (see §2.7). The integration of breakthroughs into the tradition object on completion is not yet fully wired.

**Tradition Integration Pipeline.** The intended flow from hedge mage contact to completed research is: contact seeds potential concepts into the mage's tradition; a goal is generated to pursue each novel concept; the research service generates experimental phases; research proceeds season by season; breakthrough completion integrates the concept. The goal generation step for tradition integration is not yet implemented.

**Knowledge Propagation.** Completed breakthroughs should be propagatable to other magi through teaching, training, and written works. A tractatus or lab text carrying breakthrough points should be able to seed a research idea in a reader. Neither propagation mechanism is yet implemented.

**Future Research Targets.** Shape and Material bonuses will become researchable when item enchantment is implemented. Lab Features as research targets will be addressed in the laboratory customization Phase 2 work.

**Spell Invention Phase 2.** The current spell invention system is mechanically functional but motivationally thin. The central design challenge for Phase 2 is giving magi genuine reasons to want specific spells, grounded in what is happening in and around their lives. These problem-driven motivations require the Scenario and Story Manager (see §2.9.1) to be generating world events and problems that magi can identify and respond to.

### 2.3.2. Laboratory Management

A laboratory is the primary working environment for all Hermetic research. It is built in a magical aura and contributes to all lab totals performed within it. A laboratory has several statistics: Size, Refinement, General Quality, Safety, Health, Aesthetics, Upkeep, and Warping. Features can add Art-specific or Activity-specific bonuses on top of these.

**Build Lab.** A magus without a laboratory spends a season establishing one in their covenant's aura or in any aura they control. A newly built lab has no refinement and no features.

**Refine Lab.** A magus can spend a season refining their laboratory, increasing Refinement by 1. Refinement is capped at the mage's Magic Theory minus three. During refinement, the Intelligence and personality of the mage influence whether certain features are gained or lost: a sufficiently organized mage may gain the Highly Organized virtue (+1 General Quality); a sufficiently tidy mage may gain the Spotless virtue (+1 Health, +1 Aesthetics, +1 Creo specialization); and a botched refinement season may introduce the Hidden Defect flaw (-3 Safety).

**Specialize Lab.** A laboratory can be specialized toward a single magical Art or a single Activity through four stages: Minor Focus (-1 Quality, +2 bonus) → Minor Feature (no Quality penalty, +2 bonus, requires Magic Theory 4 + Refinement 1) → Major Focus (-1 Quality, +4 bonus) → Major Feature (no Quality penalty, +4 bonus, requires Magic Theory 6 + Refinement 3). A lab can only have one specialization at a time.

**Install Lab Feature.** Discrete physical or organizational features can be added to a laboratory, each consuming available space. Currently implemented features are Highly Organized, Spotless, and Hidden Defect.

**Lab Assistants.** An apprentice currently contributes to their master's Lab Total, adding their Intelligence and Magic Theory score. Support for additional lab assistants and familiars is a planned future addition.

**Lab Refinement Phase 2.** The long-term design targets the full laboratory Virtue and Flaw system from the Covenants sourcebook. The current Phase 1 specialization system is a deliberate simplification. Phase 2 replaces it with the full taxonomy, supporting multiple simultaneous specializations (up to two activity specializations and four Art specializations, of which at most two may be Techniques), and distinguishing Focus Flaws from Features. The harder design problem is folding the full system into the AI decision engine: a mage must weigh the season cost of installing or removing a Virtue against its benefit to their specific research agenda. The belief and prestige system, once mature, should inform which specializations a mage values.

**Lab Upkeep and Covenant Economics.** Lab upkeep is currently tracked but not yet enforced as an economic constraint. Once the covenant economic system is built out (see §2.4), lab upkeep should be integrated into it.

**Lab Warping.** The Warping lab characteristic is currently tracked but not yet connected to the warping simulation system.

### 2.3.3. Study & Advancement

- **Reading:** Characters can read Summae and Tractatus to gain experience.
- **Vis Study:** Magi can expend vis to gain experience in Magic Arts.
- **Practice:** Characters can practice non-Art abilities to gain a small amount of experience.

### 2.3.4. Writing & Crafting

Characters can write Summae and Tractatus for trade or prestige. The decision to write is driven by the economic simulation: a character evaluates known demand from other magi and weighs the expected trade value against the opportunity cost of the season.

### 2.3.5. Lab Text Lifecycle

Lab texts are the primary mechanism by which magi share and reproduce magical research. Four activities are supported, each consuming one season:

**Writing.** A magus converts personal working notes into a clean, universally-readable lab text. The rate of production is determined by their score in the writing language used.

**Copying.** A magus copies an already-readable lab text. The copying rate is determined by their Profession: Scribe score.

**Deciphering Shorthand.** A magus who acquires another mage's personal shorthand notes must spend seasons deciphering them in the laboratory, accumulating progress equal to their Lab Total in the relevant Technique and Form each season. Once accumulated progress meets or exceeds the level of the text, the shorthand is understood.

**Translating.** Once a magus has decoded an author's shorthand for a given level, they may write up any of that author's texts at the standard writing rate.

### 2.3.6. Book Copying

Books — Summae and Tractatus — can be copied by any character who can read and write the language. A language score of 3 is sufficient for accurate copying. Careful copying produces a copy of identical quality to the original. Quick copying (planned) produces copies at three times the rate but one lower Source Quality. Copying is not yet wired into the AI goal system. Collection ownership and library management are deferred to Covenants Phase 2.

### 2.3.7. Social & Exploration

- **Apprenticeship:** Magi can search for, find, train, and gauntlet apprentices. Finding uses Folk Ken + Perception + Area Lore/Etiquette bonuses + Intellego Vim magic bonus. Generated apprentices receive randomized childhood ability scores. Canon requires at least one season of training per year, enforced through a training goal with a core curriculum (Magic Theory 5, Parma Magica 5, Latin 4, Artes Liberales 1, master's best Art to 5). Training desire is modulated by the master's Prudence facet. Known gaps — gauntlet after 15 years, pre-Arts training, Bonisagus claim, rule-breaking paths — are addressed in Apprentices Phase 2.

- **Recruitment:** Magi can search for and recruit hedge wizards into the Order. The pipeline has two phases: finding (Folk Ken + Perception + Area Lore/Etiquette + InVi magic bonus, against an ease factor) and persuasion (contested roll, currently defaulting to success). On success, the hedge mage converts to a Hermetic magus, joins the recruiter's master's covenant as a Visitor, and their tradition's potential breakthroughs become available as research targets.

  The Founding Scenario uses the recruitment pipeline with goal-driven behavior rather than scripting. Trianoma is initialized with a permanent `RecruitHedgeMageGoal`; her high Fairness and Sociability facets produce strong desire for it. Bonisagus responds organically when an unfamiliar tradition appears at his covenant, generating integration and Opening goals from his standing personality and goal machinery. The historical Founding emerges from the interaction of their goals and circumstances — no fixed action sequence is hard-coded. Other scenarios may seed initial conditions that make specific historical outcomes highly probable (see §2.9.1), but the outcomes themselves remain products of the simulation.

  Outstanding work for Founders Phase 2: define a HedgeTradition for each founder; implement procedural hedge mage generation; set staggered starting ages and states for founders; complete the contested recruitment roll logic; wire the breakthrough transfer from the conversion method into the research pipeline.

- **Exploration:** Characters can search for magical auras and sources of vis.

### 2.3.8. Book Title Generation

Books in the simulation are currently identified by generated placeholders. The Book Title Generation feature gives each written work a procedurally generated title that reflects its author, topic, and era — authentically medieval and Hermetic, drawing on the author's personality, reputation focuses, the topic of the work, and a vocabulary appropriate to the medieval Latin scholarly tradition.

### 2.3.9. Enchanted Items

Magi can create enchanted items as an alternative to spell invention. Items offer capabilities that spells cannot: they can be given to other characters who lack the Gift, do not require personal casting, and can be traded as economic goods.

Phase 1 covers two item types and the Verditius Outer Mystery:

**Lesser Enchantments.** A single-season lab activity producing an item with one permanent magical effect. The Lab Total must meet or exceed the effect level. The vis cost is one pawn matching either the Technique or Form for every ten levels or fraction thereof.

**Charged Items.** No vis required; completable in one season. The number of charges equals one for every five points by which the Lab Total exceeds the effect level.

**Verditius Outer Mystery.** Granted at gauntlet. Adds the Verditius mage's relevant Craft Ability to all Lab Totals when enchanting. Personally crafted items reduce the vis cost to open an invested item by the Craft score.

Invested devices, talismans, and Shape and Material bonuses are deferred to later phases. Enchanted items should be tradeable goods within the economic simulation.

---

## 2.4. Covenants

A covenant is the primary social and physical unit of Hermetic life — a shared home, magical site, and community of magi.

### 2.4.1. Covenant Model

A covenant is a distinct entity in the simulation. It is anchored to a magical aura, maintains a shared library of books and a shared stockpile of vis keyed by magical art, and is a valid subject for the belief system.

### 2.4.2. Membership & Roles

Characters can be affiliated with a covenant in one of three roles: Founder, Full Member, or Visitor. Hedge magi who have been recruited into the Order and apprentices are added to their master's covenant as Visitors. A mage who discovers a suitable magical aura and has completed basic Hermetic training may autonomously pursue a goal to found a covenant on that site.

### 2.4.3. Shared Resources

A covenant's vis stockpile is transparently pooled with a member's personal vis when evaluating whether they can afford an activity. The covenant library is accessible to members and can receive donated books, though donation decisions are governed by individual AI choices and will be more formally addressed in Covenants Phase 2.

### 2.4.4. Covenants Phase 2

The current covenant implementation is a structural skeleton. Phase 2 develops it into a living economic and social institution, covering:

**Governance.** Covenants are self-governing bodies. Governance determines how collective decisions are made — resource allocation, dispute resolution, authority over shared property. The governance model interacts directly with the legal system and the social simulation.

**Income and Expenditure.** A real covenant earns income through vis sources, mundane wealth, scribing, and trade; it spends on lab maintenance, covenfolk wages, building upkeep, and the Aegis of the Hearth ritual.

**Covenfolk.** Non-magi residents who provide services essential to covenant operation. The simulation does not yet model covenfolk as active agents.

**Boons and Hooks.** Structural advantages and disadvantages characterizing a covenant's site, fortifications, resources, and relationships. Not yet modeled.

**The Covenant as Economic Actor.** Beyond sharing vis with members, the covenant should eventually act as an economic entity in its own right — negotiating with other covenants, maintaining relationships with Redcaps, and accumulating or losing reputation as an institution.

---

## 2.5. Economic Simulation

### 2.5.1. Design Philosophy

The foundation of the economic simulation is the vis distillation rate — the amount of vis a mage can extract from their aura in a single season. This serves as the universal opportunity cost currency: every activity a mage considers is implicitly weighed against the vis they could have distilled instead. The belief system (see §2.6) modifies these baseline economic preferences.

Vis is not uniform in value. The standard trading ratio within the Order is: 1 pawn of Technique vis = 2 pawns of non-Vim Form vis = 4 pawns of Vim vis.

The current economic simulation operates with perfect information — a deliberate early-stage simplification. The intended long-term design is substantially more realistic: information will propagate imperfectly; transactions will require intermediaries such as Redcaps; and the system will model real risk — goods lost in transit, bad actors, fraud, and the general uncertainty of medieval commerce.

### 2.5.2. Supply & Demand

Each season, characters generate desires for books, lab texts, and vis based on their active goals. These desires are currently aggregated into a global market visible to all participants simultaneously.

### 2.5.3. Asset Valuation

Lab texts are valued from both the seller's and buyer's perspectives before any trade is proposed. The seller's minimum price reflects the opportunity cost of having produced the text. The buyer's valuation is the number of seasons saved versus inventing the spell from scratch, converted to vis at the buyer's distillation rate. The final price is the midpoint of the buyer's valuation and the seller's minimum price. Books are similarly evaluated.

### 2.5.4. Trade Execution

An automated trading phase runs each season. Each mage generates a trading profile capturing desires, available assets, and minimum prices. Profiles are compared pairwise across all magi. Three types of lab text trades can be proposed: buying with vis, selling for vis, or direct swap. Vis trades and book trades follow analogous patterns. All trades execute automatically when a viable offer is found.

---

## 2.6. Knowledge, Belief & Reputation

Characters build a model of the world and its inhabitants through a belief system.

### 2.6.1. Belief Profiles

Each character maintains a belief profile for every entity they have encountered. A belief profile is a collection of topic-keyed beliefs, each carrying a magnitude representing how strongly the observer holds that view. Belief profiles are asymmetric. The current belief update model is purely additive — a known placeholder. Future iterations will introduce recency weighting, primacy effects, confidence decay, and the influence of prior beliefs on new evidence.

### 2.6.2. Information Propagation

Beliefs are currently formed primarily by reading books and acquiring lab texts. Every written work carries a belief payload embedded at authoring time. On rare occasions, something of the author's personality leaks into their writing. When another character reads or acquires a work, those beliefs propagate into their profile for the author.

The current propagation mechanism reflects the limits of what has been built. The belief system is architected to eventually support richer propagation channels, including direct personal interaction and second-hand social transmission.

### 2.6.3. Prestige & Reputation Motivation

Reputation in this simulation is emergent and personal: each observer constructs their own valuation of another character based on accumulated beliefs, filtered through their own priorities. Each character carries reputation focuses — a personal weighting of which ability domains they consider prestigious. A character's prestige motivation is derived from personality: low Modesty combined with high Social Boldness produces a stronger drive toward recognition. Prestige factors into decisions about which spells to invent and which books to write, weighed alongside practical and economic value.

**Reputation Focuses — Current & Planned.** Reputation focuses are currently hand-authored for the founding characters. For general use, focuses should be procedurally generated from the character's personality profile, ability investments, and apprenticeship history. Crucially, reputation focuses are not intended to be static — they should evolve as the character's priorities shift and as they observe which accomplishments command respect among their peers.

**Reputation Phase 2.** Addresses two significant gaps: propagation beyond written works (direct interaction, second-hand transmission), and the connection between reputation and personality evolution. These two systems are deeply intertwined and should be designed together.

---

## 2.7. Magical Tradition System

This section formalizes the architecture required to support the Founding scenario and the long-term goal of a setting-agnostic tradition evolution engine.

### 2.7.1. MagicalTradition Model

A MagicalTradition is the canonical representation of what a practitioner of a given magical discipline can do. A MagicalTradition contains:

- **Known Concepts:** A set of tradition concepts representing the tradition's current capabilities (native or successfully integrated).
- **Potential Concepts:** Tradition concepts that have been observed via tradition contact but not yet integrated. These seed research projects.
- **Lineage:** A provenance string identifying the tradition's origin. Format: `"OpenerName[TraditionName]>OpenerName[TraditionName]"` — each `>` represents a re-opening event. The `Opener` field holds the live reference to the GiftedCharacter who performed the most recent Opening; the Lineage string provides denormalized historical reconstruction when ancestors are no longer in memory.
- **Spontaneous Magic Multiplier** and other foundational mechanics.

The tradition architecture is designed to eventually support tradition schisms — a single tradition splitting into two divergent branches. The lineage tracking in the MagicalTradition model is intended to support this.

### 2.7.2. TraditionConcept Model

A tradition concept is the atomic unit of magical knowledge that can be exchanged between traditions. It wraps a principle (a spell attribute, spell base, lab activity, or magical ability) and carries:

- **Source Tradition:** The tradition from which this concept originates.
- **Status:** Native | Potential | Integrated. Native concepts require no research. Potential concepts have been observed and are available as research targets. Integrated concepts have been incorporated through a completed research project.
- **Integration Difficulty:** A numeric value that influences the level and duration of experimental spell phases required to integrate it.

### 2.7.3. Tradition Contact & Integration Flow

The lifecycle of inter-tradition knowledge exchange:

- **Contact:** A magus encounters a practitioner of a different magical tradition. The source tradition's concepts are inspected.
- **Seeding:** Concepts not present in the magus's own tradition are added to potential concepts and exposed as available research targets.
- **Research:** Experimental spell phases are generated for each targeted concept. A research project is created per concept, and the magus's goal generator may produce an integration goal to pursue it.
- **Integration:** Upon breakthrough completion, the concept moves from potential to known, activating its mechanical effect.

### 2.7.4. Breakthrough Definitions

Initial required definitions include:

- **New Range Breakthrough:** Integrates a novel spell range not present in the base tradition.
- **New Spell Base Breakthrough:** Integrates a novel spell base effect combination unavailable in the base tradition.
- **New Lab Activity Breakthrough:** Integrates a novel lab activity (e.g., a hedge tradition that can extract vis differently).

Each hedge tradition ships with a pre-populated list of breakthrough definitions derived from the concepts in its magical tradition.

### 2.7.5. Mysteries

The Mystery system models initiatory magical traditions. A fundamental distinction must be respected: **Exoteric Mysteries** (the four founding Mystery Houses) and **Esoteric Mysteries** (Mystery Cults operating across House lines in secret).

**Exoteric Mysteries.** The four Mystery Houses — Bjornaer, Criamon, Merinita, and Verditius — structure their magical advancement around initiation into their House's Outer Mystery and, for dedicated members, deeper inner secrets.

**Esoteric Mysteries.** Mystery Cults are secret organizations operating across House boundaries. They are unknown to the vast majority of the Order. Their magic represents vestiges of older magical traditions never fully integrated into the Hermetic framework, newer developments from non-Hermetic sources, or reconstructed ancient rites.

**The Initiation Mechanics.** An initiation requires an Initiation Script. The Initiation Total is: Presence + Mystery Cult Lore + Script Bonus. There is no die roll — if the total meets or exceeds the target, the initiation succeeds deterministically. Target levels are 15 for a known Minor Virtue, 21 for a known Major Virtue, 18 for a new Minor Virtue (self-initiation), and 30 for a new Major Virtue. Ordeals from previous initiations reduce target levels for subsequent ones.

The Mysteries system is expected to develop across multiple phases:

- **Phase 1:** Mechanical framework for initiation + the four Exoteric Mystery Houses with canonical Outer Mystery Initiation Scripts.
- **Phase 2:** Hedge traditions that use Mystery-style initiation as their primary mode of transmitting supernatural knowledge.
- **Phase 3:** Esoteric Mystery Cults that develop organically within the Order over the course of the simulation.

---

## 2.8. Legal System

The Order of Hermes is governed by the Code of Hermes and the Peripheral Code. The Legal System is intentionally deferred until the simulation is rich enough to support it meaningfully — implementing the Code before the personality, belief, and social foundations exist would produce mechanical compliance without meaningful behavior.

The Legal System therefore depends on the personality evolution system (§2.1.1 Phase 2), the reputation and social propagation system (§2.6 Phase 2), the Scenario and Story Manager (§2.9.1), and a mature world-interaction model.

When eventually implemented, the anticipated design covers:

**The Code of Hermes.** The five clauses of the Oath and their canonical interpretations: not slaying fellow magi, not interfering with another's apprentice, not endangering the Order through mundane entanglement, not scrying on sodales, and not dealing with the Infernal. High Crimes (those that warrant death or renouncing) versus Low Crimes (those subject to lesser sanction).

**The Tribunal System.** Regional Tribunals that convene periodically, adjudicate accusations, assign punishments, and pass rulings that extend the Peripheral Code.

**Quaesitores.** The magi of House Guernicus whose role is investigation and prosecution, modulated by personality — a rigid Guernicus will prosecute everything; a pragmatic one may choose their battles.

**Wizard War.** A formal declaration of lethal intent between two magi, delivered one lunar month in advance. Wizard War is the most self-contained legal mechanic and is the natural target for Phase 1 of the Legal System. A simulation with mature personality evolution, belief profiles, and social history should be capable of producing Wizard Wars that feel earned — the result of a specific relationship deteriorating to the point of no return — rather than randomly generated.

---

## 2.9. World Simulation

The simulation's long-term goal is a world that actively engages with the magi — not just a backdrop against which they conduct their research.

### 2.9.1. Scenario & Story Manager

The simulation currently models magi working in their laboratories, venturing out to find apprentices and vis sources, and trading with one another. What it does not yet model is the world pushing back. The Scenario and Story Manager is the system responsible for generating and managing these external events.

The system is intended to operate on two levels. The first is **condition injection**: the Scenario Manager maintains a set of world-state facts that it asserts or modifies at appropriate moments — the existence of a specific character, a relationship between two factions, a piece of knowledge that has spread to certain magi. These injected conditions interact with the organic goal and desire machinery to make certain outcomes highly probable without scripting them directly. A scenario that wants the Schism War to be nearly inevitable does not schedule the war at a fixed date; it injects the preconditions — Diedne's tradition is secretive and her spontaneous magic is strategically threatening, Tremere's ambition creates accumulating resource competition, the Houses' beliefs about each other deteriorate over time — and monitors whether the simulation's organic dynamics are driving toward the expected threshold. If they are, the probability of conflict increases each tick until it fires. If by some chance the characters develop unusually cooperative histories, the war may not occur, or may occur later and for different reasons. The second level is **procedural generation**: ongoing creation of rumors, threats, opportunities, and requests that emerge organically from the state of the simulation world and give characters problems to solve or pursue.

The connection to spell invention is direct: a mage who faces a demon incursion and has no Perdo Vim spells is motivated to acquire them. Story events create spell-shaped problems, and solving problems with spells creates story. The Scenario and Story Manager is therefore a prerequisite for spell invention to become motivationally rich.

### 2.9.2. Interaction with the Mundane World

Medieval Europe is a living world of nobles, peasants, merchants, clergy, and scholars. Magi are embedded in this world whether they wish to be or not. The Mundane World system models the abstracted presence of this world and its engagement with the magi through representative archetypes and event categories.

These interactions are double-edged. The Code of Hermes explicitly prohibits magi from interfering with mundanes in ways that bring danger to the Order. At the same time, ignoring mundane problems entirely has its own costs. The mundane world also provides economic context: markets, trade routes, silver economies, and the cost of non-magical goods and services.

### 2.9.3. Interaction with Other Domains

The world of Ars Magica contains four supernatural realms — Magic, Faerie, Infernal, and Divine — each with its own denizens, agendas, and mechanics. The four realms will be treated mostly symmetrically. The Code of Hermes intersects here: dealing with the Infernal is an explicit violation of the oath, raising legal stakes for any mage who does so.

### 2.9.4. Magic Creatures

Magic creatures are beings native to or strongly associated with the Magic realm. They appear as potential familiars, guardians of vis sources, inhabitants of regiones, and targets or threats generated by the Story Manager. Magic creatures with a Might score are immune to Warping — they are already fully part of their realm.

The Magic Creatures system is greenfield. The design will need to address how creatures are generated and categorized, what their Might score represents mechanically, how they interact with Hermetic magic, and how the AI handles them as agents.

### 2.9.5. Familiars

A familiar is a magical creature that a Hermetic magus bonds with through a dedicated lab activity, creating a lifelong magical connection. The bond has three cords — Bronze (physical), Silver (mental), Gold (spiritual) — each of which can be enchanted with effects. The familiar grants additional lab assistant capability and is immune to the social penalty of the Gift.

Phase 1 covers the core bonding lab activity, the cord mechanics, and the familiar as a lab assistant. The familiar's behavior as an agent with its own personality and goals is a Phase 2 extension. Bjornaer's canonical inability to have a familiar will be factored in when the Mysteries system is implemented.

---

## 2.10. Geography & Tribunals

Geography and Tribunals are deeply intertwined in the Ars Magica setting. The Order is organized into regional Tribunals, each covering a portion of Mythic Europe, and the physical geography of those regions shapes the political realities of the Tribunals that govern them.

This system is greenfield and will be one of the later features to be implemented, as it depends on many of the systems that precede it.

**Physical Geography.** The simulation will model the information relevant to magi: travel times between covenants and population centers, the distribution and strength of magical auras across the landscape, the locations of known vis sources, and the rough topology that determines which covenants are neighbors.

**Tribunals.** The Order is divided into regional Tribunals, each holding a formal gathering approximately every seven years. Tribunals are the primary venue for Hermetic legal proceedings, political maneuvering, and collective decision-making. Each Tribunal has its own political character shaped by its history, its dominant Houses, and the personalities of its prominent magi. These differences should eventually emerge from the simulation itself rather than being hand-authored as fixed properties.

**Hermes Portals.** House Mercere maintains a network of permanent magical portals connecting major Mercer Houses and key covenants, dramatically reducing travel time for those with access.

---

## 3. Current Scope & Limitations (Out of Scope for v1.0)

**User Interface:** The project is a headless engine. The SkillViewer application serves as a debug and inspection tool, not a full-fledged user interface for managing a saga.

---

## 4. Future Considerations

- Extending the MagicalTradition system beyond the Ars Magica setting to support arbitrary fantasy worlds with distinct magical paradigms, enabling generic "tradition collision" simulations independent of the Hermetic framework. The tradition architecture is deliberately designed with this in mind.
