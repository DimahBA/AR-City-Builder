# AR City Builder
## Time System and Simulation

The game uses a simple “day” system instead of a fully continuous simulation. One in-game “day” passes every few seconds of real time (for example, every 3–5 seconds), and on each day the game updates all key values in a single tick. This makes balance easier to tune and gives players clear, readable feedback on how their decisions affect the city.

At each day tick, the following happens:
- Houses contribute population up to their capacity.
- Services consume money but improve happiness for nearby houses.
- Factories may have a one-time build cost plus optional ongoing penalties (e.g., noise/pollution).
- Commercial zones generate income based on local population and local happiness.

## Building Types and Effects

## Houses

- Provide population; each house has a fixed capacity (e.g., 10 residents).
- Population is the sum of all active, non-abandoned houses.
- Houses track a simple happiness score influenced by nearby services and factories
Houses become abandoned if happiness in their radius falls below a threshold for a few days in a row.
## Services (Hospitals, Schools, etc.)

- Cost money each day to operate.
- Provide a happiness boost to nearby houses within a certain radius or up to a certain number of closest houses
- If the city runs out of money and cannot pay the service cost, the service shuts down and its happiness bonus disappears until the economy recovers.

Services are the main way to keep residents satisfied and to sustain commercial income at high levels.

## Factories

- Have a significant one-time construction cost.
- Optionally consume a small amount of money per day to represent maintenance or wages.
- Provide economic justification for commerce (if needed in your design), but introduce negative effects like noise and pollution, lowering happiness for nearby houses.

Placing factories too close to residential areas will gradually push happiness down, causing residents to leave and indirectly hurting commercial zones.

## Commercial Zones

- Generate money each day based on nearby population and happiness.
- Example logic:
    - If nearby population is below a threshold, the zone generates little or no income.
    - If happiness of nearby houses is low, income is reduced (e.g., multiplied by 0.5)

Commercial zones are the primary income source, so the player must ensure they are supported by enough happy residents and not excessively harmed by nearby factories.
## Happiness and Feedback

Happiness is tracked per house and summarizes how livable an area is. It combines:
- Positive contributions from services in range
- Negative contributions from factories (noise, pollution) in range.
- Optional global modifiers (e.g., running a deficit for many days could slightly reduce happiness everywhere).

## TODO

## Time system

- [x] Implement a day timer that advances a day every few seconds and calls a day-end handler in a single tick.

- [x] Add methods to start and stop the day system (`StartDaySystem`, `StopDaySystem`).

- [ ] On each day tick, update *all* systems (population, happiness, services, factories, commercial) in `ProcessDayEnd`, not just commercial income.

## Money, UI, and basic buildings

- [x] Implement money, population, and day labels in the UI, with methods for adding/spending money and updating population/day display.

- [x] Implement per-building payment when placement becomes valid, including affordability checks and refund on invalidated placement.

- [x] Implement visual feedback for placement validity, adjacency to roads, and affordability (colors on the building mesh).

- [ ] Connect population calculations so the UI population label reflects the sum of active residents each day.

## Data model and building types

- [x] Extend `BuildingData` to include building type (House, Service, Factory, Commercial) instead of relying only on `buildingName` text.

- [x] Add per-type parameters: house capacity; service radius and happiness bonus; factory pollution radius and penalty; commercial base income and multipliers.

- [ ] Implement dedicated components or a central system to handle per-type logic (House, ServiceBuilding, FactoryBuilding, CommercialBuilding) rather than string checks.

## Houses and population

- [ ] Give each house a fixed capacity and track its current population (often equal to capacity unless abandoned).

- [ ] Implement a daily population update that sums all non-abandoned houses and sends the total to `GameUI.UpdatePopulation`.

- [ ] Track a happiness value per house, recalculated each day based on nearby services and factories.

- [ ] Add abandonment logic: if happiness stays below a threshold for several days, mark the house as abandoned, remove its population from the city, and disable its contribution to commercial zones.

## Services (hospitals, schools, etc.)

- [ ] Find assets for each service (a hospital, a school, a park)

- [ ] Add a daily operating cost field for service buildings and subtract it each day if the city can pay.

- [ ] Implement a service active/inactive state; disable their happiness effect when the city cannot pay the daily cost and re-enable when money recovers.

- [ ] Implement a radius- or “closest N houses”–based happiness boost each day for houses in range.

- [ ] Add visual feedback (e.g., icon or color) when a service is shut down due to lack of funds.

## Factories

- [x] Ensure factories use the generic one-time construction cost system already in `Building`.

- [ ] Implement negative happiness effects (noise/pollution) on nearby houses each day, scaled by distance or within a fixed radius.

- [ ] [optional] Add visual cues when an area is heavily polluted or when factories are hurting nearby residents’ happiness.

## Commercial zones and income

- [x] Implement a basic commercial income pass that, once per day, finds commercial buildings and grants a flat income to the player.[^1]

- [ ] Replace the flat income with a formula using nearby population and average happiness of nearby houses.

- [ ] Define “nearby” for commercial zones (radius around the building)

- [ ] Implement thresholds: low nearby population gives little or no income; low nearby happiness multiplies income down (e.g., 0.5).

- [ ] Allow tuning of base income, thresholds, and happiness multipliers via `BuildingData` so you can balance without code changes.

## Happiness system and global modifiers

- [ ] Design a central happiness calculation pipeline that, each day, aggregates impacts on each house from services, factories, and optional global modifiers.

- [ ] Implement per-house happiness storage and history (e.g., consecutive days below threshold for abandonment).

- [ ] Add optional global modifiers such as: if the city runs a deficit for many days, apply a small negative happiness modifier to all houses.

- [ ] Provide clear in-world feedback (colors, icons, or overlays) showing house happiness state (happy/ok/unhappy/abandoned).

## Integration, performance, and polish

- [ ] In `ProcessDayEnd`, call all subsystems in a clear order: update services (cost and status), recalc house happiness, update abandonment, recalc population, then calculate commercial income.

- [ ] Avoid repeated expensive calls per frame; keep `FindObjectsOfType<Building>` to the once-per-day tick or maintain central registries of buildings by type
