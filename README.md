# ObjectPoolReuseCaseDemo

- This project is originally meant for demonstrating how to arrange object lifecycles in a sense of Object Reuse Case as to avoid implementing IReset interface proposed in recently, which is a centralized solution to reset object contents to its initial state, but is not perfromance friendly, as is described here: https://github.com/dotnet/aspnetcore/pull/46426#issuecomment-1427344605

- Now this demo project is turned into a demo project for demonstrating my tiny tool: BFlatA - A wrapper/building script generator for BFlat, a native C# compiler, for recursively building .csproj file with referenced projects & Nuget packages & embedded resources, etc. You can find it here: https://github.com/xiaoyuvax/bflata 
> ObjectPoolReuseCaseDemo is a simple C# project with one Project Reference and one Nuget Package reference together with several secondary dependencies, and is a typical scenario for demonstrating how BFlata works with BFlat. More details at :https://github.com/xiaoyuvax/bflata#demo-project
