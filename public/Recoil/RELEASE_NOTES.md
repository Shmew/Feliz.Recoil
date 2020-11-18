### 0.19.2 - Wednesday, November 18th, 2020
* Support FSharp.Core 5.0

### 0.19.1 - Monday, November 9th, 2020
* Fix IDisposable when used with AtomEffects

### 0.19.0 - Saturday, November 7th, 2020
* Update for recoil 0.1.2 release

### 0.18.0 - Wednesday, September 16th, 2020
* Update for recoil 0.0.12 release

### 0.17.2 - Thursday, September 2nd, 2020
* Fix compilation issue with latest FSharp.Core

### 0.17.1 - Sunday, July 27th, 2020
* Fix async snapshot map overloads.

### 0.17.0 - Thursday, July 16th, 2020
* Unify atoms and selectors CE dangerouslyAllowMutability operation

### 0.16.3 - Thursday, July 16th, 2020
* Resolve MSBuild compiler error in the Recoil.root component

### 0.16.2 - Friday, June 19th, 2020
* Fix issue with disposal of pending time travel

### 0.16.1 - Friday, June 19th, 2020
* Ensure pending time traveling disposes on recoil root unmount

### 0.16.0 - Friday, June 19th, 2020
* Update for 0.0.10 recoil release
* Added the useTimeTravel hook

### 0.15.0 - Saturday, June 13th, 2020
* Revert 0.14 to add WriteOnly RecoilValues.

### 0.14.0 - Friday, June 12th, 2020
* Add map and bind to RecoilValue as methods.

### 0.13.0 - Thursday, June 11th, 2020
* Convert elmish API into useReducer and useSetReducer and move into main library, removes Cmd support due to potential race-condition issues.

### 0.12.0 - Wednesday, June 10th, 2020
* Simplify elmish API and improve performance.

### 0.11.0 - Monday, June 8th, 2020
* Simplify validation API and improve performance.

### 0.10.0 - Sunday, June 7th, 2020
* Added automatic local/session storage subscriptions
* Refactored the root component to be more idiomatic
* Some visibility adjustments to reduce intellisense clutter

### 0.9.0 - Sunday, June 7th, 2020
* Added overloads for useCallback and useCallbackRef for additional parameters
* Added opt-in RecoilResult and RecoilOption CE and functions
* Fixed some async/promise conversion when lifting into state
* Added Feliz.Recoil.Validation library

### 0.8.0 - Tuesday, June 2nd, 2020
* Add `RecoilValue.flatten`

### 0.7.1 - Tuesday, June 2nd, 2020
* Fix fable path resolution

### 0.7.0 - Monday, June 1st, 2020
* Update for 0.0.8 release which adds:
  * atomFamily
  * selectorFamily
  * noWait
  * waitForAll
  * waitForAny
  * waitForNone
* Refactor map/bind internals for better performance.
* Refactor elmish internals for better performance.
* Various bug fixes.

### 0.6.0 - Saturday, May 23th, 2020
* Add Elmish.Bridge adapter

### 0.5.3 - Saturday, May 23th, 2020
* Relax FSharp.Core version restrictions

### 0.5.2 - Saturday, May 23th, 2020
* Remove some recoil CE methods

### 0.5.1 - Saturday, May 23th, 2020
* Add hashing to inner selector keys to prevent collisions

### 0.5.0 - Saturday, May 23th, 2020
* Add map, bind, apply, etc for RecoilValue
* Add RecoilValue computation expression

### 0.4.0 - Saturday, May 23th, 2020
* Add Elmish companion library

### 0.3.1 - Wednesday, May 20th, 2020
* Fix visibility attributes for some types

### 0.3.0 - Wednesday, May 20th, 2020
* Finish API bindings.
* Add logging component.

### 0.2.3 - Wednesday, May 20th, 2020
* Update Feliz dependency.

### 0.2.2 - Monday, May 18th, 2020
* Fix Recoil.defaultValue initialization.

### 0.2.1 - Monday, May 18th, 2020
* Fix issue with atom CE type resolution.

### 0.2.0 - Monday, May 18th, 2020
* Fix computation expression syntax and overloads.

### 0.1.0 - Monday, May 18th, 2020
* Initial release

### 0.0.1 - Friday, May 15th, 2020
* Initial build
