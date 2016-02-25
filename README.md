# CDD.CommCentral
Client-Server routing component for the message distribution system

CommCentral:
- Accepts connections
- Authenticates clients
- Registers MessageHost endpoints
- Pairs Publishers to best-fit MessageHosts
- Stores Publisher GUIDs
- Upon Subscriber request, looks up Publisher GUID to find the serving MessageHost
- Pairs Subscriber to MessageHost which is serving the target Publisher

