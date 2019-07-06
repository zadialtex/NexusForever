-- Dumping structure for table nexus_forever_character.property_base
CREATE TABLE IF NOT EXISTS `property_base` (
  `type` int(10) unsigned NOT NULL DEFAULT '0',
  `property` int(10) unsigned NOT NULL DEFAULT '0',
  `value` float NOT NULL DEFAULT '0',
  `note` varchar(100) NOT NULL DEFAULT '',
  PRIMARY KEY (`type`,`property`)
);

-- Dumping data for table nexus_forever_character.property_base: ~12 rows (approximately)
INSERT INTO `property_base` (`type`, `property`, `value`, `note`) VALUES
	(0, 0, 0, 'Player - Base Strength'),
	(0, 1, 0, 'Player - Base Dexterity'),
	(0, 2, 0, 'Player - Base Technology'),
	(0, 3, 0, 'Player - Base Magic'),
	(0, 4, 0, 'Player - Base Wisdom'),
	(0, 7, 200, 'Player - Base HP per Level'),
	(0, 9, 500, 'Player - Base Endurance'),
	(0, 35, 18, 'Player - Base Assault Rating per Level'),
	(0, 36, 18, 'Player - Base Support Rating per Level'),
	(0, 41, 0, 'Player - Shield Capacity Base'),
	(0, 100, 1, 'Player - Base Movement Speed'),
	(0, 130, 0.8, 'Player - Base Gravity Multiplier');
	(0, 191, 1, 'Player - Base Mount Movement Speed'),