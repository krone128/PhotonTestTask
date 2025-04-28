# Photon Test task

This project implements abstract game server with following requirements:

1. Host - Client approach using Photon Fusion
2. Game scene - Flat field, divided onto square sectors, forming a grid. 
3. Player Controller - Top down camera
4. Game scene on Host is populated with entities (networked objects) on session start. Entities are constantly moving to random positions on a game scene. Entity state (Position and Destination in this case) will be replicated on clients.
5. Client keeps track of replicated entities, associated with ids, spawning a new entity if it doesn’t exist yet, pooling no longer used entities for later reuse. Client should implement some layer of simulation, reducing the amount of networked data required to consistently replicate current game state.
6. Host sends entities’ state updates to each client
7. Interest management system - Each square sector on game fields represents an interest sector.  This system decides if a particular client is interested in an entity update, based on interest radius and current player and entity sector.
8. Server keeps its update packages size as small as possible. State updates are sent for entities residing in current player interest zone only when it’s required

### Parameters

- Game scene - grid **1000x1000** units, divided by **100x100** units chunks (**10x10** chunks)
- Entities count - **5000**
- Interest zone - **3x3** block

## How to launch

The project was developed using Unity 6 on MacOS

1. Open selected project in Unity
2. Open File - Build Settings, select your standalone platform
3. Tap Build and Run button, the app will open after build

To launch multiple instances of an app

## How to test

The best way to test such a loaded system would be to either use data snapshots from update ticks, or set up a multipeer mode to make a visual test. 
However, the quickest way to test is to start multiple instances of an app in fullscreen, one of them being a server, toggle between them and see if the difference in picture is noticeable/considerable.

## Implementation details

### Project

Photon Fusion sample project was used to skip some boiler plate code (connection, player controller, etc.)

### Networking

Networking data protocol consists of a single message for the sake of simplicity. Entity update message represents an array of 1 or more of the following objects:

```jsx
//byte size - 10 bytes
EntityUpdateMessage
{
	id: short // 2b
	position: half2 //4b
	direction: half2 //4b
}
```

Networking controller also implements simple message batching. If update message size is less than a threshold setting, it will be stored and sent on subsequent updates within a set interval of ticks or when a batch size exceeds threshold size.

In result, for 5000 entities in 1000x1000 world, 10x10 chunks, average package size when not changing chunk is about 50 bytes per tick, while moving to another chunk may require up to 3000 bytes to update all entities in newly loaded chunks.

## Interest management

Interest management system will always run before sending updates to clients.
Core interest management logic uses DirtyFlag set on updated entities.

### DirtyFlags Implementation

**DirtyFlags** is set on an networked entity when it has its state was changed and its representation should be updated on clients

Entity contains 2 dirty flags

1. **IsDirty** - set to **TRUE** when entity’s Destination property was updated
2. **IsChunkDirty** - set to **TRUE** when entity moves to another interest chunk

### Interest checks implementation

To reduce update message size, Host should only send updates for entities within player’s Interest zone and marked as Dirty.

Client keeps track of updated entities, spawning a new one if it doesn’t exist yet, simulating movement based on entity Position and Destination properties. As long as an entity resides in client zone of interest, it will only receive update message once when it’s state was updated and marked as Dirty on Host.

However, there are some other cases when player may be already interested in an entity update which wasn’t marked as Dirty.

Interest Management logic implements this set of data checks to ensure consistent updates. 

1. Player moved to a different sector
**TRUE** when current player sector is different from last visited sector
**Action**: Send updates for all entities in newly visited sectors, disregarding if they are Dirty or not.
2. Player joined
**TRUE** when player doesn’t have last visited sector registered
**Action**: Send updates for all entities within its zone of interest, disregarding if they are Dirty or not.
3. Entity entered a player’s zone of interest
**TRUE** when current entity sector is different from last registered visited sector, current sector is within the zone of interest, last visited sector is out of the zone of interest.
**Action**: Send entity update, disregarding if they are Dirty or not.