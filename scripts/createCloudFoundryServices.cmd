@ECHO OFF

cf create-service p-config-server standard myConfigServer -c config-server.json
cf create-service p-service-registry standard myDiscoveryService 
cf create-service p.mysql db-small myFortuneDB
cf create-service p-redis shared-vm myRedisService
cf create-service p-circuit-breaker-dashboard standard myHystrixService
cf cups AttendeeContext -p "{\"connectionstring\": \"myconnectionstring\"}"
