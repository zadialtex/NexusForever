-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               8.0.13 - MySQL Community Server - GPL
-- Server OS:                    Win64
-- HeidiSQL Version:             9.5.0.5196
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Dumping structure for table nexus_forever_world.store_category
CREATE TABLE IF NOT EXISTS `store_category` (
  `id` int(10) unsigned NOT NULL DEFAULT '0",
  `parentId` int(10) unsigned NOT NULL DEFAULT '26",
  `name` varchar(50) NOT NULL DEFAULT '",
  `description` varchar(150) NOT NULL DEFAULT '",
  `index` int(10) unsigned NOT NULL DEFAULT '1",
  `visible` tinyint(1) unsigned NOT NULL DEFAULT '0",
  PRIMARY KEY (`id`),
  KEY `parentId` (`parentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table nexus_forever_world.store_category: ~0 rows (approximately)
/*!40000 ALTER TABLE `store_category` DISABLE KEYS */;
INSERT INTO `store_category` (`id`, `parentId`, `name`, `description`, `index`, `visible`) VALUES
	(26, 0, "TOP LEVEL", "DO NOT DELETE", 0, 0),
	(27, 26, "Holo-Wardrobe", "Costumes, weapons, and dyes keep you safe and looking sharp.", 7, 1),
	(28, 27, "Costumes", "Look like a quadrillion space-bucks when you update your style with new costume pieces.", 1, 1),
	(29, 27, "Weapons", "Need a new weapon? Find deadly tools of combat to suit every class here.", 3, 1),
	(31, 26, "Mounts", "Because walking everywhere is for suckers.", 9, 1),
	(32, 31, "Hoverboards", "Go anywhere and shred everywhere with gravity-defying hoverboards.", 2, 1),
	(33, 31, "Ground Mounts", "From sleek beasts to overpowered hoverbikes, you'll cover more ground than ever with these mounts.", 1, 1),
	(34, 26, "Consumables", "Eat, drink, and be merry with these premium consumables.", 12, 1),
	(35, 26, "Unlocks", "Expand your horizons (and your options) with character and account unlocks.", 13, 1),
	(36, 35, "Account Unlocks", "Get new character slots, increase your décor limit, access additional costumes, and more.", 1, 1),
	(37, 35, "Character Unlocks", "Enhance a character's personal life with options like extra bank slots and improved riding skills.", 2, 1),
	(39, 27, "Dyes", "Color-coordinate or dress in every color of the rainbow. The choice is yours.", 4, 1),
	(41, 31, "Flair", "Customize your rides with a variety of interchangeable mount flairs.", 3, 0),
	(43, 34, "Flasks", "Need a boost to your XP, reputation, harvesting, or monetary gain? Get a tall, cool drink of awesome here.", 1, 1),
	(44, 34, "Services", "Need to craft a rune or visit the bank in a hurry? Consumable services are the answer.", 2, 1),
	(46, 26, "Crafting", "Become a master of tradeskills and runecrafting with these bundles.", 14, 1),
	(47, 46, "Convenience", "All the tools you need to get the most out of your tradeskills.", 1, 1),
	(48, 46, "Rune Services", "Effortlessly improve your runecrafting with handy rune services items. ", 2, 1),
	(49, 26, "Keys and Currencies", "Buy things that let you get other things, including Fortune Coins, Lockbox Keys, and service tokens.", 15, 1),
	(50, 26, "Bundles", "Score an intergalactic deal with WildStar bundles!", 4, 1),
	(76, 26, "Featured", "Featured", 1, 0),
	(78, 26, "Pets", "Who says you can't buy friendship? Pets will keep you company during your Nexus adventures.", 11, 1),
	(79, 26, "Housing", "Make your housing plot your own with new décor, remodel options, and FABkits.", 8, 1),
	(80, 79, "Décor", "Become a decorated hero of the housing community with exclusive décor bundles.", 1, 1),
	(81, 79, "Remodel", "The sky's no limit with these premium housing customization options.", 2, 1),
	(82, 79, "FABkits", "Functional, flashy, and fully automated, FABkits provide new gameplay options for your housing plot.", 3, 1),
	(130, 26, "Toys", "For fun, bragging rights, and just plain showing off.", 10, 1),
	(132, 26, "Limited Time", "Get 'em before they're gone!", 5, 1),
	(206, 50, "Signature Packs", "Purchase signature time with NCoin!", 1, 1),
	(207, 50, "Special Offers", "Check out the shop's latest bargain bundles!", 2, 1),
	(212, 26, "Beginner Basics", "New to WildStar? Check out our shiny new deals for bright beginnings!", 6, 1),
	(214, 26, "Signature Station", "Special offers for Signature players", 2, 1),
	(215, 27, "Costume Pieces", "", 2, 1),
	(216, 26, "Protobucks Conversion", "", 17, 0),
	(242, 26, "Black Friday", "Celebrate Black Friday with these deals (and steals)!", 3, 0);
/*!40000 ALTER TABLE `store_category` ENABLE KEYS */;
