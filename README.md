# Kaori Mau - Система управления рестораном японской кухни

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?logo=windows)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-316192?logo=postgresql)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?logo=dotnet)

**Kaori Mau** — это десктопное приложение для управления рестораном японской кухни, разработанное на WPF с использованием паттерна MVVM. Приложение позволяет клиентам просматривать меню, создавать кастомные роллы и сеты, оформлять заказы, а администраторам — управлять меню, пользователями и заказами.

## 📋 Содержание

- [Основные возможности](#основные-возможности)
- [Технологический стек](#технологический-стек)
- [Архитектура проекта](#архитектура-проекта)
- [Установка и настройка](#установка-и-настройка)
- [Структура проекта](#структура-проекта)
- [Использование](#использование)
- [База данных](#база-данных)
- [Разработка](#разработка)

## ✨ Основные возможности

### Для клиентов:
- 🔐 **Регистрация и авторизация** — безопасная система входа с хешированием паролей
- 📱 **Просмотр меню** — каталог роллов и сетов с изображениями, описаниями и ценами
- 🎨 **Создание кастомных блюд** — возможность создавать собственные роллы и сеты из доступных ингредиентов
- 🛒 **Корзина покупок** — добавление товаров в корзину с возможностью изменения количества
- 📦 **Оформление заказов** — выбор даты и времени доставки (до 3 дней вперед), типа доставки (самовывоз/курьер)
- 📊 **История заказов** — просмотр всех своих заказов с детальной информацией
- 👤 **Профиль пользователя** — управление личными данными и смена пароля

### Для администраторов:
- 👥 **Управление пользователями** — просмотр, редактирование, блокировка пользователей
- 🍣 **Управление меню** — добавление, редактирование и удаление позиций меню
- 📋 **Управление заказами** — просмотр всех заказов, изменение статусов заказов
- 📈 **Статистика** — отслеживание количества роллов, сетов и заказов

## 🛠 Технологический стек

- **.NET 8.0** — платформа разработки
- **WPF (Windows Presentation Foundation)** — UI фреймворк
- **Entity Framework Core 9.0** — ORM для работы с базой данных
- **PostgreSQL** — реляционная база данных
- **Npgsql 9.0.4** — провайдер PostgreSQL для .NET
- **MvvmHelpers** — библиотека для упрощения работы с MVVM
- **MvvmLightLibs** — дополнительные инструменты для MVVM

## 🏗 Архитектура проекта

Проект построен на основе **паттерна MVVM (Model-View-ViewModel)**, что обеспечивает разделение логики представления и бизнес-логики.

### Основные компоненты:

#### 1. **MVVM Pattern**
- **Model** (`Tables/`) — сущности базы данных
- **View** (`Views/`) — XAML разметка и code-behind
- **ViewModel** (`ViewModels/`) — логика представления и связь с моделью

#### 2. **Repository Pattern**
- Абстракция доступа к данным через интерфейсы
- Разделение бизнес-логики и работы с БД
- Репозитории для каждой сущности: `UserRepository`, `MenuItemRepository`, `OrderRepository` и др.

#### 3. **NavigationService**
- Кастомный сервис навигации для переключения между страницами
- Регистрация страниц через фабрики
- Кэширование страниц для оптимизации

#### 4. **Support Classes**
- **Value Converters** — конвертеры для привязки данных (Boolean, Visibility, Enum)
- **Cart** — класс для управления корзиной покупок
- **CurrentUser** — статический класс для хранения данных текущего пользователя
- **RollViewModel** — универсальная модель для отображения позиций меню

## 📦 Установка и настройка

### Требования:
- Windows 10/11
- .NET 8.0 SDK
- PostgreSQL 15+ (или выше)
- Visual Studio 2022 или Rider (рекомендуется)

### Шаги установки:

1. **Клонируйте репозиторий:**
   ```bash
   git clone https://github.com/yourusername/kaori-mau.git
   cd kaori-mau
   ```

2. **Настройте базу данных PostgreSQL:**
   - Создайте базу данных:
     ```sql
     CREATE DATABASE "KaoriMau";
     ```
   - Создайте enum типы:
     ```sql
     CREATE TYPE order_status AS ENUM ('pending', 'accepted', 'preparing', 'delivering', 'ready_for_pickup', 'done', 'canceled');
     CREATE TYPE delivery_type AS ENUM ('pickup', 'courier');
     ```
   - Примените миграции или создайте таблицы вручную (см. структуру БД ниже)

3. **Настройте подключение к БД:**
   Отредактируйте файл `ООП_Курсовой/App.config`:
   ```xml
   <appSettings>
       <add key="DbHost" value="localhost"/>
       <add key="DbPort" value="5432"/>
       <add key="DbName" value="KaoriMau"/>
   </appSettings>
   ```
   
   В файле `DatabaseContext.cs` измените строку подключения (по умолчанию: `Username=postgres;Password=1111`)

4. **Восстановите зависимости:**
   ```bash
   dotnet restore
   ```

5. **Соберите проект:**
   ```bash
   dotnet build
   ```

6. **Запустите приложение:**
   ```bash
   dotnet run --project ООП_Курсовой/ООП_Курсовой.csproj
   ```

## 📁 Структура проекта

```
ООП_Курсовой/
├── Tables/              # Модели данных (Entity Framework)
│   ├── User.cs
│   ├── MenuItem.cs
│   ├── Order.cs
│   ├── Category.cs
│   ├── Ingredient.cs
│   ├── enums.cs
│   └── DatabaseContext.cs
├── Repository/          # Репозитории для работы с БД
│   ├── IRepository.cs
│   ├── UserRepository.cs
│   ├── MenuItemRepository.cs
│   ├── OrderRepository.cs
│   └── ...
├── ViewModels/          # ViewModels для MVVM
│   ├── MainWindowViewModel.cs
│   ├── LoginViewModel.cs
│   ├── MenuRollsWindowViewModel.cs
│   ├── CartWindowViewModel.cs
│   ├── AdminWindowViewModel.cs
│   └── ...
├── Views/               # XAML представления
│   ├── MainWindowView.xaml
│   ├── LoginView.xaml
│   ├── AdminWindowView.xaml
│   ├── Pages/          # Страницы приложения
│   │   ├── HomePage.xaml
│   │   ├── MenuRollsPage.xaml
│   │   ├── MenuSetsPage.xaml
│   │   ├── CartPage.xaml
│   │   └── ...
│   └── ...
├── SupportClasses/      # Вспомогательные классы
│   ├── NavigationService.cs
│   ├── Cart.cs
│   ├── CurrentUser.cs
│   ├── RollViewModel.cs
│   └── Converters/     # Value Converters
├── App.xaml            # Точка входа приложения
├── App.config          # Конфигурация (настройки БД)
└── ООП_Курсовой.csproj  # Файл проекта
```

## 💻 Использование

### Первый запуск:

1. При первом запуске приложение откроет главное окно
2. Для доступа к функциям необходимо авторизоваться или зарегистрироваться
3. Нажмите кнопку "Войти" в главном окне

### Создание администратора:

Для создания первого администратора необходимо выполнить SQL запрос:
```sql
INSERT INTO roles (id, name) VALUES (1, 'Admin'), (2, 'Client');

INSERT INTO users (login, phone, email, password_hash, firstname, lastname, role_id, created_at, is_blocked)
VALUES ('admin', '+79991234567', 'admin@kaorimau.ru', 
        'хеш_пароля', 'Администратор', 'Системы', 1, NOW(), false);
```

**Важно:** Пароль должен быть захеширован. В текущей реализации используется хеширование через `BCrypt` или аналогичный алгоритм.

### Основные функции:

- **Навигация:** Используйте боковую панель для перехода между разделами
- **Добавление в корзину:** Нажмите на карточку товара и выберите количество
- **Создание кастомного ролла:** Перейдите в "Мои роллы" → "Создать ролл"
- **Оформление заказа:** Перейдите в корзину и нажмите "Оформить заказ"

## 🗄 База данных

### Основные таблицы:

- **users** — пользователи системы
- **roles** — роли (Admin, Client)
- **menu_items** — позиции меню (роллы и сеты)
- **categories** — категории меню
- **ingredients** — ингредиенты для роллов
- **ingredient_rolls** — связь ингредиентов и роллов (many-to-many)
- **orders** — заказы
- **order_items** — позиции в заказе
- **menu_set_components** — компоненты сетов

### Enum типы:

- **order_status**: `pending`, `accepted`, `preparing`, `delivering`, `ready_for_pickup`, `done`, `canceled`
- **delivery_type**: `pickup`, `courier`

### Связи:

- Пользователь может иметь множество заказов (1:N)
- Позиция меню может содержать множество ингредиентов (N:M через `ingredient_rolls`)
- Сет может содержать множество позиций меню (N:M через `menu_set_components`)
- Заказ может содержать множество позиций (1:N через `order_items`)

## 🔧 Разработка

### Паттерны и практики:

1. **MVVM:**
   - ViewModels наследуются от `BaseViewModel` (MvvmHelpers)
   - Команды реализованы через `Command` и `AsyncCommand`
   - Привязка данных через `Binding` в XAML

2. **Repository Pattern:**
   - Каждая сущность имеет свой репозиторий
   - Репозитории реализуют интерфейс `IRepository<T>`
   - Изоляция логики доступа к данным

3. **Navigation:**
   - Навигация через `NavigationService`
   - Регистрация страниц в `MainWindowView.InitializeNavigation()`
   - Переход через команды ViewModel

4. **Commands и Actions:**
   - **Commands** (`ICommand`) — для действий пользователя (кнопки, меню)
   - **Actions** (`Action<T>`) — для коммуникации между ViewModels и Views
   - Пример: `OpenLoginRequested`, `RequestClose`

5. **Value Converters:**
   - `BoolToVisibilityConverter` — преобразование bool в Visibility
   - `EnumToBoolConverter` — для RadioButton с enum значениями
   - `NullableLongToBoolConverter` — для выбора категорий
   - `StringToVisibilityConverter` — условное отображение элементов

### Работа с данными:

- **Entity Framework Core** используется для работы с БД
- **NpgsqlDataSource** для пула соединений
- **Enum mapping** для PostgreSQL enum типов
- **Value Converters** для преобразования enum в строки

### Стилизация:

- Темная тема (#111111, #000000)
- Акцентный цвет: #FF4D00 (оранжево-красный)
- Закругленные углы для кнопок и карточек
- Адаптивная верстка с использованием Grid и StackPanel

## 📝 Лицензия

Этот проект создан в учебных целях для курсовой работы по объектно-ориентированному программированию.

## 👤 Автор

Разработано в рамках курсовой работы по ООП.

---

**Примечание:** Для работы приложения необходимо наличие PostgreSQL базы данных с корректной структурой таблиц и enum типов. Убедитесь, что настройки подключения в `App.config` и `DatabaseContext.cs` соответствуют вашей конфигурации БД.

