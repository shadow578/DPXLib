# DPXLib
DPXLib is a C# library to interact with Catalogic DPX. <br>
The library ues the new REST api added in Version 4.6, which was manually reverse- engineered by sniffing the traffic while interacting with the web interface.
Because of this, it's possible that there are functions not implemented, or implemented wrong.

## Compatibility
Compatibility is __only__ tested with the latest version i have access to. <br>
This currently is: __4.9.0__
<br>
<br>
DPXLib may be compatible with other versions of DPX, tho that is not something i'll test.<br>
Also, i can only access my setup, so any features that i don't use won't be tested (at least not by me :P).

## Features
DPXLib allows querying of licensing information, nodes and node-groups, as well as job instances and their logs.<br>
Due to being able to access the job logs, multiple functions were implemented to query:
- Tape Media / Volsers used by a job instance
- Job durations for Initializing, Waiting and Writing

### Endpoints
The following API Endpoints are implemented (GET only)
- /auth/login
- /license
- /job_instances
- /job_instances/{id}
- /job_instances/{id}/log
- /job_instance_statuses/{status}
- /nodes
- /node_groups
- /node_groups/{name}

# Disclaimer
This is very much experimental and should __not__ be used in production!