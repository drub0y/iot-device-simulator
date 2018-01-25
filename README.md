# IoT Device Simulator for Load Scenarios

## Example Script file for C#
``` csharp
            // TODO: these definitions need to come from a configuraiton file...
            var randomizer = new Randomizer();

            var truckData = new Faker<TruckData>()
                .RuleFor(td => td.DeviceId, device.Id)
                .RuleFor(td => td.DeviceType, deviceType)
                .RuleFor(td => td.Latitude, f => f.Address.Latitude())
                .RuleFor(td => td.Longitude, f => f.Address.Longitude())
                .Generate();

            var twin = await deviceClient.GetTwinAsync();
            if (twin == null)
            {
                twin = new Twin(device.Id);
            }

            twin.Tags["IsSimulated"] = "Y";
            twin.Properties.Desired["Latitude"] = truckData.Latitude;
            twin.Properties.Reported["Latitude"] = truckData.Latitude;
            twin.Properties.Desired["Longitude"] = truckData.Longitude;
            twin.Properties.Reported["Longitude"] = truckData.Longitude;

            await registryManager.UpdateTwinAsync(device.Id, twin, "*");

            ServiceEventSource.Current.ServiceMessage(Context, $"Sending data for {truckData.DeviceId}");
            while (true)
            {
                truckData.Latitude.TweakValue(5);
                truckData.Longitude.TweakValue(5);

                var messageJson = JsonConvert.SerializeObject(truckData);
                var encodedMessage = Encoding.ASCII.GetBytes(messageJson);
                await deviceClient.SendEventAsync(new Message(encodedMessage));

                ServiceEventSource.Current.ServiceMessage(Context, $"Sending message for {truckData.DeviceId}");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
```