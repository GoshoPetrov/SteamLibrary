# SteamLibrary

## Какво представлява системата?

Системата "SteamLibrary" е конзолно приложение за управление на библиотека от игри. Тя позволява на потребителите да се регистрират, да влизат в системата и да управляват своите игри. Системата има три нива на достъп: администратор, потребител и гост.

- **Администраторите** могат да управляват потребителите (преглед и изтриване).
- **Потребителите** могат да управляват игри (преглед, добавяне, изтриване) и да извършват импорт/експорт на данни в JSON формат.
- **Гостите** могат само да разглеждат каталога с игри.

## Описание на ентититата и връзките

### Списък на класовете

-   **User**: Представлява потребител на системата. Всеки потребител има потребителско име, имейл, хеширана парола и ниво на достъп.
-   **Access**: Представлява ниво на достъп (напр. "Administrator", "User", "Guest").
-   **Game**: Представлява игра в библиотеката. Всяка игра има заглавие, описание, жанр, цена, дата на издаване и др.
-   **Publisher**: Представлява издател на игри. Всеки издател има име, местоположение, имейл и т.н.
-   **UserGame**: Свързваща таблица между `User` и `Game`, която показва кои игри притежава даден потребител.

### Връзки между тях

-   **User - Access**: One-to-many (един потребител има едно ниво на достъп, едно ниво на достъп може да има много потребители).
-   **Game - Publisher**: One-to-many (една игра има един издател, един издател може да има много игри).
-   **User - Game**: Many-to-many (един потребител може да притежава много игри, една игра може да бъде притежавана от много потребители). Връзката се осъществява чрез `UserGame`.
-   **User - Game (AddedBy)**: One-to-many (един потребител може да е добавил много игри).
-   **User - Publisher (CreatedBy)**: One-to-many (един потребител може да е създал много издатели).

## Описание на основните LINQ справки

-   **List all users** (Меню за администратори -> опция 1):
    -   Стартира се от `ManageUsersScreen` -> `ListAllUsers`.
    -   Използва `Logic.LoadAllUsers(null)`.
    -   Показва списък с всички потребители в системата и техните нива на достъп.
-   **List the available games** (Меню за потребители/гости -> опция 1):
    -   Стартира се от `BrowseGamesScreen` -> `ShowGameList`.
    -   Използва `Logic.LoadAllGames()`.
    -   Показва списък с всички игри в каталога и техните издатели.
-   **Search by title** (Меню за потребители/гости -> опция 2):
    -   Стартира се от `BrowseGamesScreen` -> `SearchGame`.
    -   Използва `Logic.LoadAllGames(filter)`.
    -   Показва списък с игри, чието заглавие съдържа въведения от потребителя текст.

## Описание на JSON форматите

### Експорт

При експорт се създава JSON файл със следната структура:

```json
{
  "exportDate": "YYYY-MM-DDTHH:mm:ss.sssssssZ",
  "games": [
    {
      "id": integer,
      "title": string,
      "description": string (nullable),
      "genre": string (nullable),
      "price": decimal,
      "releaseDate": "YYYY-MM-DD",
      "ageRating": integer (nullable),
      "isMultiplayer": boolean,
      "publisher": {
        "id": integer,
        "name": string,
        "location": string (nullable)
      } (nullable),
      "addedBy": {
        "id": integer,
        "userName": string,
        "email": string
      } (nullable),
      "users": [
        {
          "userId": integer,
          "userName": string,
          "addedDate": "YYYY-MM-DD HH:mm:ss",
          "isFavorite": boolean,
          "purchasePrice": decimal (nullable)
        }
      ],
      "createdAt": "YYYY-MM-DDTHH:mm:ss.sssssssZ",
      "updatedAt": "YYYY-MM-DD HH:mm:ss" (nullable)
    }
  ],
  "publishers": [
    {
      "id": integer,
      "name": string,
      "location": string (nullable),
      "email": string (nullable),
      "phone": string (nullable),
      "foundedDate": "YYYY-MM-DD",
      "createdBy": {
        "id": integer,
        "userName": string,
        "email": string
      } (nullable),
      "gameCount": integer,
      "gameTitles": [ string ],
      "createdAt": "YYYY-MM-DDTHH:mm:ss.sssssssZ",
      "updatedAt": "YYYY-MM-DD HH:mm:ss" (nullable)
    }
  ]
}
```

### Импорт

При импорт се очаква JSON файл със същата структура. Полетата `id` се използват за намиране на съществуващи записи и обновяването им. Ако запис с такова `id` не съществува, се създава нов.

## Инструкции за стартиране

### Как да се пусне приложението

1.  Отворете решението `SteamLibrary.sln` с Visual Studio.
2.  Изберете `SteamLibrary` като стартов проект.
3.  Натиснете `F5` или бутона `Start`, за да компилирате и стартирате приложението.
