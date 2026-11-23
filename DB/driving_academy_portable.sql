-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: localhost    Database: driving_academy
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `cars`
--

/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cars` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cars`
--

/*!40000 ALTER TABLE `cars` DISABLE KEYS */;
INSERT INTO `cars` VALUES (4,'Audi R8 LMS GT3'),(6,'Bentley Continental GT3'),(1,'BMW M4 GT3'),(2,'Ferrari 296 GT3'),(8,'Ford Mustang GT3'),(11,'Lexus RC F GT3'),(10,'McLaren 720S GT3'),(3,'Mercedes-AMG GT3'),(9,'Nissan GT-R GT3');
/*!40000 ALTER TABLE `cars` ENABLE KEYS */;

--
-- Table structure for table `lap_results`
--

/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `lap_results` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int unsigned NOT NULL,
  `track_configuration_id` int unsigned NOT NULL,
  `car_id` int unsigned NOT NULL,
  `lap_time_ms` int unsigned NOT NULL,
  `driven_at` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_result_car` (`car_id`),
  KEY `ix_leaderboard` (`track_configuration_id`,`car_id`,`user_id`,`lap_time_ms`),
  KEY `ix_user_history` (`user_id`,`driven_at`),
  CONSTRAINT `fk_result_car` FOREIGN KEY (`car_id`) REFERENCES `cars` (`id`),
  CONSTRAINT `fk_result_configuration` FOREIGN KEY (`track_configuration_id`) REFERENCES `track_configurations` (`id`),
  CONSTRAINT `fk_result_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`),
  CONSTRAINT `ck_positive_time` CHECK ((`lap_time_ms` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=28 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `lap_results`
--

/*!40000 ALTER TABLE `lap_results` DISABLE KEYS */;
INSERT INTO `lap_results` VALUES (10,8,1,1,93410,'2026-11-08 08:43:21'),(11,9,1,1,86440,'2026-11-05 23:26:21'),(12,10,1,1,126678,'2026-10-31 01:19:21'),(13,11,1,1,98931,'2026-11-20 03:03:21'),(14,12,1,1,109589,'2026-11-10 08:23:21'),(15,13,2,2,111816,'2026-11-05 00:45:21'),(16,14,1,1,103209,'2026-11-19 18:31:21'),(17,15,4,4,99636,'2026-11-14 08:34:21'),(18,16,3,3,109209,'2026-11-01 01:18:21'),(19,17,2,2,98244,'2026-11-19 06:00:21'),(20,18,1,1,118756,'2026-11-12 11:09:21'),(22,20,3,3,117565,'2026-10-28 20:23:21'),(23,8,2,2,99138,'2026-10-29 07:53:21'),(24,9,1,1,105539,'2026-11-04 21:52:21');
/*!40000 ALTER TABLE `lap_results` ENABLE KEYS */;

--
-- Table structure for table `roles`
--

/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id` tinyint unsigned NOT NULL,
  `name` varchar(15) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'Пилот'),(2,'Админ клуба'),(3,'Системный админ'),(4,'Судья'),(5,'Организатор'),(6,'Наблюдатель'),(7,'Маршал'),(8,'Инструктор'),(9,'Механик'),(10,'Тренер'),(11,'Комментатор');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;

--
-- Table structure for table `track_configurations`
--

/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `track_configurations` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `track_id` int unsigned NOT NULL,
  `name` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_configuration` (`track_id`,`name`),
  CONSTRAINT `fk_configuration_track` FOREIGN KEY (`track_id`) REFERENCES `tracks` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `track_configurations`
--

/*!40000 ALTER TABLE `track_configurations` DISABLE KEYS */;
INSERT INTO `track_configurations` VALUES (1,1,'Grand Prix'),(21,1,'Короткий круг'),(2,2,'Grand Prix'),(11,3,'Full Circuit'),(3,4,'Full Circuit'),(8,5,'Full Circuit'),(6,6,'Full Circuit'),(9,7,'Full Circuit'),(12,8,'Full Circuit'),(4,9,'Full Circuit'),(7,10,'Full Circuit'),(10,11,'Full Circuit'),(5,12,'Full Circuit');
/*!40000 ALTER TABLE `track_configurations` ENABLE KEYS */;

--
-- Table structure for table `tracks`
--

/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tracks` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tracks`
--

/*!40000 ALTER TABLE `tracks` DISABLE KEYS */;
INSERT INTO `tracks` VALUES (4,'Barcelona'),(9,'Bathurst'),(12,'Brands Hatch'),(6,'Imola'),(10,'Interlagos'),(5,'Laguna Seca'),(1,'Monza'),(7,'Red Bull Ring'),(11,'Silverstone'),(2,'Spa-Francorchamps'),(3,'Suzuka'),(8,'Zandvoort');
/*!40000 ALTER TABLE `tracks` ENABLE KEYS */;

--
-- Table structure for table `users`
--

/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `role_id` tinyint unsigned NOT NULL DEFAULT '1',
  `username` varchar(20) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `name` varchar(20) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `surname` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `password` varchar(255) NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`),
  KEY `fk_users_role` (`role_id`),
  CONSTRAINT `fk_users_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (8,1,'demo_pilot_1','Павел','Орлов','ca701298e68811f19255','2026-11-22 18:24:21'),(9,1,'demo_pilot_2','Олег','Соколов','ca7024cfe68811f19255','2026-11-22 18:24:21'),(10,1,'demo_pilot_3','Яна','Фомина','ca7025e7e68811f19255','2026-11-22 18:24:21'),(11,1,'demo_pilot_4','Пётр','Серов','ca702699e68811f19255','2026-11-22 18:24:21'),(12,1,'demo_pilot_5','Иван','Петров','ca70273de68811f19255','2026-11-22 18:24:21'),(13,1,'demo_pilot_6','Ирина','Белова','ca7027d9e68811f19255','2026-11-22 18:24:21'),(14,1,'demo_pilot_7','Вера','Крылова','ca70286fe68811f19255','2026-11-22 18:24:21'),(15,1,'demo_pilot_8','Марк','Власов','ca702903e68811f19255','2026-11-22 18:24:21'),(16,1,'demo_pilot_9','Лев','Громов','ca7029a9e68811f19255','2026-11-22 18:24:21'),(17,1,'demo_pilot_10','Игорь','Морозов','ca702a42e68811f19255','2026-11-22 18:24:21'),(18,1,'demo_pilot_11','Нина','Котова','ca702ae0e68811f19255','2026-11-22 18:24:21'),(19,1,'demo_pilot_12','Анна','Волкова','ca702b80e68811f19255','2026-11-22 18:24:21'),(20,1,'demo_pilot_13','Дина','Мирова','ca702c1de68811f19255','2026-11-22 18:24:21'),(23,2,'admin','admin','admin','pbkdf2-v1$vdPAVhUG6gTKhPe+Ot7wXQ==$ICh6hX8ST6scKLbUgDELzESoy27volAwyYx1JY22eBU=','2026-11-22 18:32:22'),(24,3,'sysadmin','sysadmin','sysadmin','pbkdf2-v1$sf63esjZdnwDUKjg0wX+bw==$1+IXfzxC7hCxuv8WvMW5XIYsAvVXjDhnbg5+qo596KU=','2026-11-23 02:02:29');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-23  2:58:25
