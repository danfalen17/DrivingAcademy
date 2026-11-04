USE driving_academy;
START TRANSACTION;
INSERT INTO roles (id, name) VALUES
    (1, 'Пилот'),
    (2, 'Админ клуба'),
    (3, 'Системный админ');
INSERT INTO tracks (id, name) VALUES (1, 'Monza'), (2, 'Spa-Francorchamps');
INSERT INTO track_configurations (id, track_id, name) VALUES
    (1, 1, 'Grand Prix'), (2, 2, 'Grand Prix');
INSERT INTO cars (id, name) VALUES (1, 'BMW M4 GT3'), (2, 'Ferrari 296 GT3');
COMMIT;
