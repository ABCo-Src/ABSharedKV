# Communication

### LoadSegment

Requests for a segment to be loaded. If the segment is already loaded in the current domain, this will simply return the ID of the segment as it is already loaded.

The **request**:

- The byte `1`. 
- Standard ABSave `string`, representing the name of the segment we wish to load.

The **response**:

- A `short`
  - `65521` for "segment not found".
  - `65522` for "segment found but failed to load".
  - Above `65522` for unknown errors.
  - Any other value represents the code for the segment.

### CreateSegment

Creates a new segment, with the given name. The new segment will be empty.

The **request**:

- The byte `2`. 
- Standard ABSave `string`, representing the new name to give the segment we wish to create.

The **response**:

- A `short`
  - Above `65520` for unknown errors.
  - Any other value represents the code for the segment.

### DeleteSegmentById

Deletes a currently loaded segment but its current ID.

The **request**:

- The byte `3`. 
- Standard ABSave `short`, representing the ID of the segment to delete.

The **response**:

- A `byte`
  - `0` for success.
  - Above `0` for unknown errors.

### DeleteSegmentByName

Deletes a segment by its name. It may be loaded or unloaded.

The **request**:

- The byte `4`.
- Standard ABSave `string`, representing the name of the segment to delete.

The **response**:

- A `byte`
  - `0` for success.
  - Above `0` for unknown errors.

### Add

Adds an key-value with the given key to the given segment.

The **request**:

- The byte `16`.
- Standard ABSave `short`, representing the ID of the segment to add into.
- Standard ABSave `string`, representing the key to give the new item.
- Standard ABSave `byte[]`, representing the bytes to put into the value.

The **response**:

- A `short`
  - `65521` for "item already exists".
  - Above `65521` for unknown errors.
  - Any other value represents the code for the item.

### Load

Adds an key-value with the given key to the given segment.

The **request**:

- The byte `17`.
- Standard ABSave `short`, representing the ID of the segment to add into.
- Standard ABSave `string`, representing the key of the pair to load.

The **response**:

- A `short`
  - `65521` for "item not found".
  - Above `65521` for unknown errors.
  - Any other value represents the ID for the item.
- A `byte[]`, representing the contents of the item at the position.

### Edit

Edits the value within a loaded key-value item.

The **request**:

- The byte `18`.
- Standard ABSave `short`, representing the ID of the segment to add into.
- Standard ABSave `byte[]`, representing new value to provide.

The **response**:

- A `short`
  - `65521` for "invalid ID".
  - Above `65521` for unknown errors.
  - Any other value represents the ID for the item.

### Invalid

If a request is sent starting with an unrecognized byte, the communication should be terminated for safety, with the `int` value `0xFFFFFFFF` written in response (because this value should always represent an unknown error not matter what response the client was hoping to get).