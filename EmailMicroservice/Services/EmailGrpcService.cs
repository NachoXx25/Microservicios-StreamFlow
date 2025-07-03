using Grpc.Core;
using EmailMicroservice;
using Serilog;
using Google.Protobuf.WellKnownTypes;

namespace EmailMicroservice.Services;

public class GreeterService : Greeter.GreeterBase
{

    public override Task<CheckHealthResponse> CheckHealth(Empty request, ServerCallContext context)
    {
        Log.Information("Recibida petición para verificar la salud del servicio");
        try
        {
            Log.Information($"Estado del servicio: {true}");
            return Task.FromResult(new CheckHealthResponse { IsRunning = true });
        }
        catch (Exception ex)
        {
            Log.Error($"Error al verificar la salud del servicio: {ex.Message}");
            return Task.FromException<CheckHealthResponse>(
                new RpcException(new Status(StatusCode.Internal, $"Error al verificar la salud del servicio: {ex.Message}")));
        }
    }
}
