class Tag < ActiveRecord::Base
	has_and_belongs_to_many :articles
	has_many :articles

	 validates :tag, length: { maximum: 50 }
end
