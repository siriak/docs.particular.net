---
title: Deploying ServiceControl Monitoring instances using containers
reviewed: 2024-07-02
---

ServiceControl Monitoring instances are deployed using the [`particular/servicecontrol-monitoring` image](https://hub.docker.com/r/particular/servicecontrol-monitoring), as shown in this minimal example using `docker run`:

```shell
docker run -d -p 33633:33633 -e TRANSPORTTYPE=RabbitMQ.QuorumConventionalRouting -e CONNECTIONSTRING="host=host.docker.internal" particular/servicecontrol-monitoring:latest
```

## Required settings

The following environment settings are required to run a monitoring instance:

| Environment Variable | Description |
|-|-|
| `TRANSPORTTYPE` | Determines the message transport used to communicate with message endpoints. See [TODO](TODO) for valid TransportType values. |
| `CONNECTIONSTRING` | Provides the connection information to connect to the chosen transport. The form of this connection string is different for every message transport. See [ServiceControl transport support]](/servicecontrol/transports) for more details on options available to each message transport. |
| `PARTICULARSOFTWARE_LICENSE` | The Particular Software license. The environment variable should contain the full multi-line contents of the license file. |

## Ports

`33633` is the canonical port exposed by the monitoring instance API within the container, though this port can be mapped to any desired external port.

## Volumes

The monitoring instance is stateless and does not require any mounted volumes.

## Additional settings

// TODO: Link to full settings page, describing how environment settings are understood by containerized apps