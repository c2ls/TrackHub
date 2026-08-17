/**
 * Copyright (c) 2026 Sergio Hernandez. All rights reserved.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License").
 *  You may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

/**
 * Share field flags are disclosure decisions, so their DEFAULTS are part of the
 * contract, not a cosmetic detail. `includeRoute` in particular exists because
 * the planned route used to be handed out unconditionally — if it ever defaults
 * back to true, every link silently discloses the route again (spec 11 §7.8).
 *
 * Comparing the dialog against itself is not enough, and that gap is how
 * `includeRoute` came to be missing from the `TripShareFields` fragment while
 * every dialog-internal check stayed green: `FIELD_FLAGS` matched
 * `DEFAULT_FLAGS`, and neither knew the backend had a sixth flag. So the suite
 * is anchored to the two GENERATED artefacts instead — the schema-derived input
 * type and the fragment document actually sent — which makes a seventh flag
 * added server-side a failure here rather than an invisible disclosure.
 */

import { Kind } from 'graphql';
import type { FieldNode, FragmentDefinitionNode } from 'graphql';
import { DEFAULT_FLAGS, FIELD_FLAGS } from 'layouts/tripmanager/components/ShareDialog';
import type { FieldFlagName } from 'layouts/tripmanager/components/ShareDialog';
import { TripShareFieldsFragment } from 'api/tripManagement/tripsOperations';
import type {
  TripShareFieldFlagsDtoInput,
  TripShareFieldsFragment as TripShareFieldsFragmentType,
} from 'api/tripManagement/generated/graphql';

/** Every `include*` member of a generated type — i.e. every disclosure flag. */
type FlagKeys<T> = Extract<keyof T, `include${string}`>;

/** `true` only when the two unions are mutually assignable, i.e. identical. */
type Identical<A, B> = [A] extends [B] ? ([B] extends [A] ? true : false) : false;

/**
 * COMPILE-TIME gate (`tsc --noEmit` is a build step, so this is enforced even
 * though the assignment looks inert at runtime): the flags the dialog offers
 * must be exactly the flags the backend accepts. Adding `includeXyz` to the
 * schema and regenerating turns these into type errors until the dialog offers
 * it, gives it a default, and the fragment reads it back.
 */
const dialogMatchesCommandInput: Identical<
  FlagKeys<TripShareFieldFlagsDtoInput>,
  FieldFlagName
> = true;
const dialogMatchesQueriedShare: Identical<
  FlagKeys<TripShareFieldsFragmentType>,
  FieldFlagName
> = true;

/** The `include*` fields the `TripShareFields` fragment really asks the server for. */
const fragmentFlagFields = (): string[] => {
  const definition = TripShareFieldsFragment.definitions.find(
    (node): node is FragmentDefinitionNode => node.kind === Kind.FRAGMENT_DEFINITION
  );
  if (!definition) throw new Error('TripShareFields fragment definition not found');
  return definition.selectionSet.selections
    .filter((selection): selection is FieldNode => selection.kind === Kind.FIELD)
    .map((selection) => selection.name.value)
    .filter((name) => name.startsWith('include'));
};

describe('trip share field flags', () => {
  test('the planned route is opt-in and starts unticked', () => {
    expect(DEFAULT_FLAGS.includeRoute).toBe(false);
  });

  test('every personal-data flag fails closed', () => {
    expect(DEFAULT_FLAGS.includeDriverName).toBe(false);
    expect(DEFAULT_FLAGS.includeVehicle).toBe(false);
    expect(DEFAULT_FLAGS.includePodSummary).toBe(false);
  });

  test('every flag the dialog renders has a default, and vice versa', () => {
    expect([...FIELD_FLAGS].sort()).toEqual(Object.keys(DEFAULT_FLAGS).sort());
  });

  test('the dialog offers exactly the flags the backend defines', () => {
    // These hold the compile-time assertions above; the assignments are what
    // actually fail when the backend grows a flag, and asserting them here
    // keeps the constants from being pruned as dead code.
    expect(dialogMatchesCommandInput).toBe(true);
    expect(dialogMatchesQueriedShare).toBe(true);
  });

  test('the fragment reads back every flag the dialog can set', () => {
    // A flag the dialog SENDS but never SELECTS is invisible after creation:
    // the "Existing links" list cannot say what a live link discloses, so it
    // cannot be revoked for the right reason. That was the includeRoute bug.
    expect(fragmentFlagFields().sort()).toEqual([...FIELD_FLAGS].sort());
  });
});
