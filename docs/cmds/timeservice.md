# Alice -Your Discord Assistant-
## Help - TimeService

### Commands
- [time](#time)
- [set timezone](#settimezone)
- [set sleep](#setsleep)

### Time
Usage: `!time` or `!time <Mention>`

|Version|Permission|Limit|
|:--|:--|:--|
|[001](/changelog/001.md)|[Everyone](/permissions/permissions.md)|No|

**Before Use This Command:** You or Target have to [set timezone](#settimezone) first.

This command will show time in you or target timezone.

### SetTimeZone
Usage: `!set timezone <Offset>`

|Version|Permission|Limit|
|:--|:--|:--|
|[001](/changelog/001.md)|[Everyone](/permissions/permissions.md)|Offset is required|

You should be use [correct offset format](#offsetformat).

This command will set your timezone.

#### OffsetFormat
Offset value have to in range of (-12:00 ~ +13:00)

Formats are should be like these:
- `00:00` it means UTC
- `00:30` it means UTC+0:30
- `09:00` it means UTC+9
- `09:30` it means UTC+09:30
- `-09:00` it means UTC-9
- `-09:30` it means UTC-09:30

### SetSleep
Usage: `!set sleep <Time>`

|Version|Permission|Limit|
|:--|:--|:--|
|[002](/changelog/002.md)|[Everyone](/permissions/permissions.md)|Time is required|

**Before Use This Command:** You or Target have to [set timezone](#settimezone) first.

You should be use [correct time format](#timeformat).

This command will set notification sleep time.

#### TimeFormat
Time value have to in range of (00:00 ~ 23:59)

It only allow 24H.