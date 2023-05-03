# Login System

Authentication system for TTMC applications

## Installation

Download the repo and build the solution with:

```bash
dotnet build
```
Add the builded dll file to your project
```bash
Solution Explorer > Dependencies > Add Project Reference > Browse > OK
```

## Usage

### Using
```csharp
using TTMC.LoginSystem;
```

### Register
```csharp
bool success = Account.Register("Username", "Password");
```

### Login
```csharp
Token? token = Account.Login("Username", "Password");
```

### Auth
```csharp
Guid? myid = Account.Auth(token.accessToken);
```

### Refresh
```csharp
Token? token = Account.Refresh(token.refreshToken);
```

## Example
```csharp
using TTMC.LoginSystem;

namespace Test
{
	class Program
	{
		private static void Main(string[] args)
		{
			bool success = Account.Register("Username", "Password");
			Console.WriteLine("Registered: " + success);
			Token? token = Account.Login("Username", "Password");
			if (token != null)
			{
				DoWork(token);
				token = Account.Refresh(token.refreshToken);
				Console.WriteLine("Token refreshed!");
				if (token != null)
				{
					DoWork(token);
				}
			}
		}
		private static void DoWork(Token token)
		{
			Console.WriteLine("Access Token: " + Convert.ToBase64String(token.accessToken));
			Console.WriteLine("Refresh Token: " + Convert.ToHexString(token.refreshToken));
			Guid? myid = Account.Auth(token.accessToken);
			if (myid != null)
			{
				Console.WriteLine("Your GUID: " + myid.Value);
			}
		}
	}
}
```