# servicename=abc
# host=127.0.0.2
# port=10

namespace csharp Fanews.TableSync.Thrift
namespace netcore Fanews.TableSync.Thrift


struct ResultModel {
  1: bool Success,
  2: string Message,
}

struct TableJsonData{
   1: string Json,
   2: optional string Token,
}

/**
 * Ahh, now onto the cool part, defining a service. Services just need a name
 * and can optionally inherit from another service using the extends keyword.
 */
service TableSyncService {
   i32 add(1:i32 num1, 2:i32 num2);
   ResultModel TableSync_Update(1:TableJsonData model);
}