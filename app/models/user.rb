class User < ActiveRecord::Base
has_and_belongs_to_many :tags
	has_many :articles, dependent: :destroy
	
	  before_save { self.email = email.downcase }
	  before_create :create_remember_token
    
	

	  private

 def User.new_remember_token
    SecureRandom.urlsafe_base64
  end

  def User.encrypt(token)
   Digest::SHA1.hexdigest(token.to_s)

    #BCrypt::Password.new(self.password_digest)  == Digest::SHA1.hexdigest(token.to_s)
    
  end

  private

    def create_remember_token
      self.remember_token = User.encrypt(User.new_remember_token)
    end

 def feed
    # Это предварительное решение. См. полную реализацию в "Following users".
    Article.where("user_id = ?", id)
  end
	
	validates :password_digest, length: { minimum: 6 }
	#validates :password, presence: true, confirmation: true, length: { minimum: 6 }
	validates :nick, presence: true, uniqueness: { case_sensitive: false }
	validates :login, presence: true, uniqueness: { case_sensitive: false }
	VALID_EMAIL_REGEX = /\A[\w+\-.]+@[a-z\d\-.]+\.[a-z]+\z/i
	validates :email, presence: true, format: { with: VALID_EMAIL_REGEX }, uniqueness: { case_sensitive: false }
	
	has_secure_password
end
