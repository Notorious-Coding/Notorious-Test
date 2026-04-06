-- Table des utilisateurs
CREATE TABLE Users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    email NVARCHAR(100) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    created_at DATETIME DEFAULT GETDATE()
);

-- Table des projets
CREATE TABLE Projects (
    project_id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(MAX),
    created_at DATETIME DEFAULT GETDATE(),
    owner_id INT,
    FOREIGN KEY (owner_id) REFERENCES Users(user_id) ON DELETE SET NULL
);

-- Table des tâches
CREATE TABLE Tasks (
    task_id INT IDENTITY(1,1) PRIMARY KEY,
    project_id INT NOT NULL,
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    status VARCHAR(20) CHECK (status IN ('Pending', 'In Progress', 'Completed')) DEFAULT 'Pending',
    due_date DATE,
    assigned_to INT,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (project_id) REFERENCES Projects(project_id) ON DELETE CASCADE,
    FOREIGN KEY (assigned_to) REFERENCES Users(user_id) ON DELETE SET NULL
);

-- Table des commentaires sur les tâches
CREATE TABLE Comments (
    comment_id INT IDENTITY(1,1) PRIMARY KEY,
    task_id INT NOT NULL,
    user_id INT NOT NULL,
    content NVARCHAR(MAX) NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (task_id) REFERENCES Tasks(task_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

-- Table des rôles des utilisateurs dans les projets
CREATE TABLE ProjectRoles (
    role_id INT IDENTITY(1,1) PRIMARY KEY,
    project_id INT NOT NULL,
    user_id INT NOT NULL,
    role VARCHAR(20) CHECK (role IN ('Owner', 'Member', 'Viewer')) DEFAULT 'Member',
    FOREIGN KEY (project_id) REFERENCES Projects(project_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);