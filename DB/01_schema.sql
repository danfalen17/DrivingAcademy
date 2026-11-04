CREATE DATABASE driving_academy CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE driving_academy;

CREATE TABLE roles (
    id TINYINT UNSIGNED PRIMARY KEY,
    name NVARCHAR(15) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE users (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    role_id TINYINT UNSIGNED NOT NULL DEFAULT 1,
    username NVARCHAR(20) NOT NULL UNIQUE,
    name NVARCHAR(20) NOT NULL,
    surname NVARCHAR(100) NOT NULL,
    password NVARCHAR(20) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_users_role FOREIGN KEY (role_id) REFERENCES roles(id)
) ENGINE=InnoDB;

CREATE TABLE tracks (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name NVARCHAR(30) NOT NULL UNIQUE
) ENGINE=InnoDB;

CREATE TABLE track_configurations (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    track_id INT UNSIGNED NOT NULL,
    name NVARCHAR(30) NOT NULL,
    CONSTRAINT uq_configuration UNIQUE (track_id, name),
    CONSTRAINT fk_configuration_track FOREIGN KEY (track_id) REFERENCES tracks(id)
) ENGINE=InnoDB;

CREATE TABLE cars (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name NVARCHAR(30) NOT NULL UNIQUE
) ENGINE=InnoDB;

CREATE TABLE lap_results (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id INT UNSIGNED NOT NULL,
    track_configuration_id INT UNSIGNED NOT NULL,
    car_id INT UNSIGNED NOT NULL,
    lap_time_ms INT UNSIGNED NOT NULL,
    driven_at DATETIME NOT NULL,
    CONSTRAINT ck_positive_time CHECK (lap_time_ms > 0),
    CONSTRAINT fk_result_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT fk_result_configuration FOREIGN KEY (track_configuration_id) REFERENCES track_configurations(id),
    CONSTRAINT fk_result_car FOREIGN KEY (car_id) REFERENCES cars(id),
    INDEX ix_leaderboard (track_configuration_id, car_id, user_id, lap_time_ms),
    INDEX ix_user_history (user_id, driven_at)
) ENGINE=InnoDB;
