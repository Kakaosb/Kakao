class User < ActiveRecord::Base
	  before_save { self.email = email.downcase }
	has_many :articles
	
	validates :password_digest, length: { minimum: 6 }
	#validates :password, presence: true, confirmation: true, length: { minimum: 6 }
	validates :nick, presence: true, uniqueness: { case_sensitive: false }
	validates :login, presence: true, uniqueness: { case_sensitive: false }
	VALID_EMAIL_REGEX = /\A[\w+\-.]+@[a-z\d\-.]+\.[a-z]+\z/i
	validates :email, presence: true, format: { with: VALID_EMAIL_REGEX }, uniqueness: { case_sensitive: false }
	
	#has_secure_password
end
