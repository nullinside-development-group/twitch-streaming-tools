# twitch-streaming-tools

Tools to aid twitch streamers.

## Design

### Account Management

Relationship between the view model and the account manager used to keep Twitch OAuth credentails up-to-date

```mermaid
classDiagram
    AccountViewModel o-- IAccountManager
    IAccountManager
    class AccountViewModel{
        -IAccountManager _accountManager
        +LaunchOAuthBrowser()
        +DeleteCredentials()
    }
    class IAccountManager{
        +bool CredentialsAreValid
        +Action<bool> OnCredentialStatusChanged
        +void UpdateCredentials(string bearer, string refresh, DateTime expires)
        +void DeleteCredentials()
        -void CheckCredentials()
    }
```